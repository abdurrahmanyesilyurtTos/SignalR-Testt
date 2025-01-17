public interface ICodeService
{
    Task<string> GenerateAndStoreCode();
    Task<bool> CheckLogin(string code);

}
