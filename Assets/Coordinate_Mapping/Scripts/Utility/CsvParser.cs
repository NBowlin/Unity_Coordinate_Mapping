using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoordinateMapper {
    public class CsvParser {
        static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
        static char[] TRIM_CHARS = { '\"' };

        public async static Task<Dictionary<string, object[]>> ParseAsync(string text) {
            return await Task.Run(() => Parse(text));
        }

        public static Dictionary<string, object[]> Parse(string text) {
            var info = new Dictionary<string, object[]>();
            var trimmed = text.Trim();
            var lines = Regex.Split(trimmed, LINE_SPLIT_RE);

            if (lines.Length <= 1) return info;

            var headers = Regex.Split(lines[0], SPLIT_RE);
            foreach (string header in headers) {
                info[header] = new object[lines.Length - 1];
            }

            for (var i = 1; i < lines.Length; i++) {
                var values = Regex.Split(lines[i], SPLIT_RE); //TODO: This is the bottleneck - It's like 99% of parse time
                if (values.Length == 0 || values[0] == "") { continue; }

                for (var j = 0; j < headers.Length && j < values.Length; j++) {
                    string value = values[j];
                    value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                    object finalvalue = value;
                    long n;
                    float f;
                    if (long.TryParse(value, out n)) { finalvalue = n; }
                    else if (float.TryParse(value, out f)) { finalvalue = f; }
                    var key = headers[j];
                    var keyVal = info[key];
                    keyVal[i - 1] = finalvalue;
                    info[key] = keyVal;
                }
            }
            return info;
        }
    }
}

