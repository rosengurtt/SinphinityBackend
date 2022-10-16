namespace SinphinitySysStoreApi.Helpers
{
    public static class Hashing
    {
        public static string CreateMD5(string myText)
        {
            var hash = System.Security.Cryptography.MD5.Create()
                .ComputeHash(System.Text.Encoding.ASCII.GetBytes(myText ?? ""));
            return string.Join("", Enumerable.Range(0, hash.Length).Select(i => hash[i].ToString("x2")));
        }
    }
}
