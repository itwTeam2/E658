using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace E658.HelperServices
{
    public class HashingService
    {
        // Method to hash multiple values
        public string EncodeMultipleValues(int creatorId, int roleId, int eFlowId)
        {
            // Concatenate the values into a single string (with delimiters for clarity)
            string concatenatedValues = $"{creatorId}:{roleId}:{eFlowId}";

            // Convert to Base64 (encoding, which can be reversed)
            byte[] bytes = Encoding.UTF8.GetBytes(concatenatedValues);
            return Convert.ToBase64String(bytes);
        }

        public string DecodeHashId(string encodedString)
        {
            // Convert Base64 string back to original values
            byte[] base64EncodedBytes = Convert.FromBase64String(encodedString);
            return Encoding.UTF8.GetString(base64EncodedBytes); // This will return the original concatenated string
        }
    }
}