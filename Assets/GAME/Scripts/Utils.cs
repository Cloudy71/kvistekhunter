using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

public class Utils {
    public static string TimeToString(float seconds) {
        int minutes = (int) Mathf.Floor(seconds / 60f);
        int secs = (int) Mathf.Floor(seconds - minutes * 60f);

        return (minutes < 10 ? "0" : "") + minutes + ":" + (secs < 10 ? "0" : "") + secs;
    }

    private static void CopyTo(Stream src, Stream dest) {
        byte[] bytes = new byte[4096];

        int cnt;

        while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0) {
            dest.Write(bytes, 0, cnt);
        }
    }

    public static byte[] Compress(string input) {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        using (MemoryStream msi = new MemoryStream(bytes))
            using (MemoryStream mso = new MemoryStream()) {
                using (GZipStream gs = new GZipStream(mso, CompressionMode.Compress)) {
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
    }

    public static string Decompress(byte[] bytes) {
        using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream()) {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress)) {
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
    }

    public static string StringCompress(string input, params string[] replacement) {
        for (var i = 0; i < replacement.Length; i++) {
            int start = 0;
            int end;
            string repl = replacement[i];
            string find = repl.Replace("\\", "");

            while ((start = input.IndexOf(find, start, StringComparison.Ordinal)) > -1) {
                end = start + find.Length;
                string nw = repl;
                int nwStart = nw.IndexOf("\\", StringComparison.Ordinal);
                int nwEnd = nw.LastIndexOf("\\", StringComparison.Ordinal);
                nw = nw.Substring(0, nwStart) + "\\" + i + "\\" + nw.Substring(nwEnd + 1, nw.Length - nwEnd - 1);
                input = input.Substring(0, start) + nw + input.Substring(end, input.Length - end);
                start++;
            }
        }

        Debug.Log("COMPRESS: " + input);

        return input;
    }

    public static string StringDecompress(string input, params string[] replacement) {
        for (var i = 0; i < replacement.Length; i++) {
            int start = 0;
            int end;
            string repl = replacement[i];
            string find = repl.Replace("\\", "");
            string nw = repl;
            int nwStart = nw.IndexOf("\\", StringComparison.Ordinal);
            int nwEnd = nw.LastIndexOf("\\", StringComparison.Ordinal);
            nw = nw.Substring(0, nwStart) + "\\" + i + "\\" + nw.Substring(nwEnd + 1, nw.Length - nwEnd - 1);

            while ((start = input.IndexOf(nw, start, StringComparison.Ordinal)) > -1) {
                end = start + nw.Length;
                input = input.Substring(0, start) + find + input.Substring(end, input.Length - end);
                start++;
            }
        }

        Debug.Log("DECOMPRESS: " + input);

        return input;
    }
}