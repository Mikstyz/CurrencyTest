namespace User.Entities
{
    public class EUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Dictionary<string, decimal> Wallet {get; set;}
    }
}