using System;
using System.Collections.Generic;
using System.Text;

namespace TowerDefense.EditorTools
{
    /// <summary>CSV 텍스트와 행·칸 목록 사이의 변환 (RFC 4180 방식의 따옴표 처리).</summary>
    public static class CsvText
    {
        public static string Write(IEnumerable<IReadOnlyList<string>> rows)
        {
            var text = new StringBuilder();
            foreach (IReadOnlyList<string> row in rows)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    if (i > 0) text.Append(',');
                    text.Append(Escape(row[i] ?? string.Empty));
                }
                text.Append("\r\n");
            }
            return text.ToString();
        }

        public static List<List<string>> Parse(string text)
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var cell = new StringBuilder();
            bool quoted = false;
            int start = text.Length > 0 && text[0] == '﻿' ? 1 : 0;

            for (int i = start; i < text.Length; i++)
            {
                char c = text[i];
                if (quoted)
                {
                    if (c != '"')
                    {
                        cell.Append(c);
                    }
                    else if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        cell.Append('"');
                        i++;
                    }
                    else
                    {
                        quoted = false;
                    }
                }
                else if (c == '"' && cell.Length == 0)
                {
                    quoted = true;
                }
                else if (c == ',')
                {
                    row.Add(cell.ToString());
                    cell.Length = 0;
                }
                else if (c == '\r' || c == '\n')
                {
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                    row.Add(cell.ToString());
                    cell.Length = 0;
                    rows.Add(row);
                    row = new List<string>();
                }
                else
                {
                    cell.Append(c);
                }
            }

            if (quoted) throw new FormatException("따옴표가 닫히지 않았습니다.");

            // 마지막 줄에 줄바꿈이 없는 경우
            if (cell.Length > 0 || row.Count > 0)
            {
                row.Add(cell.ToString());
                rows.Add(row);
            }
            return rows;
        }

        private static string Escape(string value)
        {
            bool needsQuotes = value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0;
            return needsQuotes ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
        }
    }
}
