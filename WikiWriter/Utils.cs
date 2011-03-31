using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace WikiWriter
{
    public static class Utils
    {
        public static string ReadFile(string filename)
        {
            using (var streamReader = new StreamReader(filename)) return streamReader.ReadToEnd();
        }

        public static void WriteFile(string filename, string contents)
        {
            using (var streamWriter = new StreamWriter(filename)) streamWriter.Write(contents);
        }

        public static string GetHash(string text)
        {
            return text.GetHashCode().ToString("x8");
        }

        public static IDictionary<string, object> GetUserRegistry(string key)
        {
            Dictionary<string, object> result = null;
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(key);
            if (registryKey != null)
            {
                result = new Dictionary<string, object>();
                foreach (string name in registryKey.GetValueNames())
                {
                    result.Add(name, registryKey.GetValue(name));
                }
                registryKey.Close();
            }
            return result;
        }

        public static void SetUserRegistry(string key, IDictionary<string, object> pairs)
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(key);
            if (registryKey == null)
            {
                throw new Exception("cannot create registry key: " + key);
            }
            foreach (string name in pairs.Keys)
            {
                registryKey.SetValue(name, pairs[name]);
            }
            registryKey.Close();
        }

        public static IEnumerable<string> GetFilesRecursive(string path)
        {
            var queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    foreach (var subDir in Directory.GetDirectories(path))
                    {
                        queue.Enqueue(subDir);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(path);
                }
                catch
                {
                }
                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        yield return files[i];
                    }
                }
            }
        }
    }
}
