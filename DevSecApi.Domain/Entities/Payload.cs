using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DevSecApi.Domain.Entities;

public class Payload
{
    public Payload() { }
    private Payload(string selector, string attribute, string url_b64, string encrypted_text_bytes_b64, string key_bytes_b64, string page_b64)
    {
        Selector = selector;
        Attribute = attribute;
        Url_b64 = url_b64;
        Encrypted_text_bytes_b64 = encrypted_text_bytes_b64;
        Key_bytes_b64 = key_bytes_b64;
        Page_b64 = page_b64;
    }
    public string Selector { get; set; } = string.Empty;
    public string Attribute { get; set; } = string.Empty;
    public string Url_b64 { get; set; } = string.Empty;
    [JsonPropertyName("encrypted_text_bytes_b64")]
    public string Encrypted_text_bytes_b64 { get; set; } = string.Empty;
    [JsonPropertyName("key_bytes_b64")]
    public string Key_bytes_b64 { get; set; } = string.Empty;
    public string Page_b64 { get;       set; } = string.Empty;
    public static Payload Create(string selector, string attribute, string url_a64, string encrypted_text_bytes_a64, string key_bytes_b64, string page_a64)
    {
        return new Payload(selector, attribute, url_a64, encrypted_text_bytes_a64, key_bytes_b64, page_a64);
                    
    }
       
}
