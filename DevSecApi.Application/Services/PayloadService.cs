using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevSecApi.Domain.Repositories;
using DevSecApi.Domain.Entities;
using AngleSharp.Html.Parser;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using FluentValidation;

namespace DevSecApi.Application.Services;

public class PayloadIncomingValidator : AbstractValidator<Payload>
{
    public PayloadIncomingValidator()
    {
        RuleFor(x => x.Url_b64).NotEmpty().WithErrorCode("MISSING_PARAMETER").WithMessage("Отсутствует параметр Url_b64");
        RuleFor(x => x.Page_b64).NotEmpty().WithErrorCode("MISSING_PARAMETER").WithMessage("Отсутствует параметр Page_b64");
        RuleFor(x => x.Key_bytes_b64).NotEmpty().WithErrorCode("MISSING_PARAMETER").WithMessage("Отсутствует параметр Key_bytes_b64");
        RuleFor(x => x.Encrypted_text_bytes_b64).NotEmpty().WithErrorCode("MISSING_PARAMETER").WithMessage("Отсутствует параметр Encrypted_text_bytes_b64");

        RuleFor(x => x.Selector).NotEmpty().WithErrorCode("EMPTY_SELECTOR").WithMessage("Пустой селектор во входящем объекте");
        RuleFor(x => x.Attribute).NotEmpty().WithErrorCode("EMPTY_ATTRIBUTE").WithMessage("Пустой атрибут во входящем объекте");
    }
}
public class PayloadService
{
    private readonly IPayloadRepository _payloadRepository;
    private readonly PayloadIncomingValidator _validator;

    private static readonly Regex EmailRegex = new Regex(
        @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public PayloadService(IPayloadRepository payloadRepository)
    {
        _payloadRepository = payloadRepository;
        _validator = new PayloadIncomingValidator();
    }
    public async Task<Payload?> DecodeAsync(Guid id)
    {
        return null;
    }
    public async Task<ProcessTaskResponse> ProcessPayloadAsync(Payload request)
    {
        if(string.IsNullOrEmpty(request.Encrypted_text_bytes_b64) || string.IsNullOrEmpty(request.Key_bytes_b64))
{
            return new ProcessTaskResponse
            {
                IsError = 1,
                ErrorCode = "MISSING_PARAMETER",
                ErrorMessage = "Криптографические параметры запроса равны null или пусты."
            };
        }
        if (request == null)
        {
            return new ProcessTaskResponse
            {
                IsError = 1,
                ErrorCode = "MISSING_REQUEST",
                ErrorMessage = "Входящий объект равен null"
            };
        }

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var firstError = validationResult.Errors.First();
            return new ProcessTaskResponse
            {
                IsError = 1,
                ErrorCode = firstError.ErrorCode,
                ErrorMessage = firstError.ErrorMessage
            };
        }

        string decodedUrl = string.Empty;
        string decodedPage = string.Empty;
        byte[] encryptedBytes = Array.Empty<byte>();
        byte[] keyBytes = Array.Empty<byte>();

        int elementsCount = 0;
        var elementsAttrList = new List<string>();
        var elementsForDb = new List<DevSecApi.Domain.Entities.PageElement>();

        int emailsCount = 0;
        var emailsList = new List<string>();
        string decryptedPlainText = string.Empty;


        try
        {
            byte[] urlBytes = Convert.FromBase64String(request.Url_b64);
            decodedUrl = System.Text.Encoding.UTF8.GetString(urlBytes);
        }
        catch (FormatException)
        {
            return new ProcessTaskResponse
            {
                IsError = 1,
                ErrorCode = "BASE64_DECODE_ERROR",
                ErrorMessage = "Ошибка при декодинге base64 в URL"
            };
        }
   
        try
        {
            byte[] pageBytes = Convert.FromBase64String(request.Page_b64);
            decodedPage = System.Text.Encoding.UTF8.GetString(pageBytes);
        }
        catch (FormatException)
        {
            return new ProcessTaskResponse 
            { 
                IsError = 1, 
                ErrorCode = "PAGE_BASE64_DECODE_ERROR",
                ErrorMessage = "Ошибка при декодинге base64 в Page"
            };
        }
        try
        {
            encryptedBytes = Convert.FromBase64String(request.Encrypted_text_bytes_b64);
            keyBytes = Convert.FromBase64String(request.Key_bytes_b64);

        }
        catch (FormatException ex)
        {
            return new ProcessTaskResponse
            {
                IsError = 1,
                ErrorCode = "CRYPTO_BASE64_DECODE_ERROR",
                ErrorMessage = $"Ошибка декодирования крипто-параметров из Base64: {ex.Message}"
            };
        }

        try
        {
            var parser = new AngleSharp.Html.Parser.HtmlParser();
            var document = parser.ParseDocument(decodedPage);

            var htmlElements = document.QuerySelectorAll(request.Selector);
            elementsCount = htmlElements.Length;

            foreach (var element in htmlElements)
            {
                string? attrValue = element.GetAttribute(request.Attribute);
                elementsAttrList.Add(attrValue ?? string.Empty);

                elementsForDb.Add(new DevSecApi.Domain.Entities.PageElement
                {
                    AttributeValue = attrValue ?? string.Empty,
                    FullHtml = element.OuterHtml
                });
            }

            
            var emailMatches = EmailRegex.Matches(decodedPage);
            foreach (Match match in emailMatches)
            {
                if (!emailsList.Contains(match.Value))
                {
                    emailsList.Add(match.Value);
                }
            }

            emailsCount = emailMatches.Count;
            if (elementsForDb.Count > 0)
            {
                await _payloadRepository.AddRangeAsync(elementsForDb);
            }
            try
            {
                if (keyBytes == null || keyBytes.Length != 32)
                {
                    decryptedPlainText = "Ошибка: Неверная длина ключа шифрования. Ожидается 32 байта (256 бит).";
                }
                else if (encryptedBytes == null || encryptedBytes.Length == 0 || encryptedBytes.Length % 16 != 0)
                {
                    decryptedPlainText = "Ошибка:Длина зашифрованного текста должна быть кратна 16 байтам для PaddingMode.None.";
                }
                else
                { 
                    using (System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create())
                    {
                        aes.Key = keyBytes;
                        aes.Mode = CipherMode.ECB;
                        aes.Padding = PaddingMode.None;

                        using (ICryptoTransform decryptor = aes.CreateDecryptor())
                        {
                            byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                            //decryptedPlainText = System.Text.Encoding.UTF8.GetString(decryptedBytes).TrimEnd('\0');

                            string rawText = System.Text.Encoding.UTF8.GetString(decryptedBytes);
                            decryptedPlainText = rawText?.TrimEnd('\0') ?? string.Empty;

                        }
                    }
                }
            }

            catch (CryptographicException ex)
            {
                decryptedPlainText = $"Криптографическая ошибка AES: {ex.Message}";
            }
            
            return new ProcessTaskResponse
            {
                IsError = 0,
                ErrorCode = "",
                ErrorMessage = "",
                ElementsCount = elementsCount,
                EmailsCount = emailsCount, 
                Url = decodedUrl,
                DecryptedPlainText = decryptedPlainText, 
                ElementsAttrList = elementsAttrList,
                EmailsList = emailsList
            };
        }

        catch (Exception ex) 
        {
            return new ProcessTaskResponse
            {
                IsError = 1,
                ErrorCode = "UNEXPECTED_ERROR",
                ErrorMessage = ex.Message,
                ElementsCount = 0,
                EmailsCount = 0,
                Url = "",
                DecryptedPlainText = "",
                ElementsAttrList = new List<string>(),
                EmailsList = new List<string>()
            };
        }


    }
}
