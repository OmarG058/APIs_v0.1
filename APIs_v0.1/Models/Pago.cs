using MongoDB.Bson.Serialization.Attributes;

namespace APIs_v0._1.Models
{
    public class Pago
    {
        [BsonElement("Fecha")]
        public DateTime Fecha { get; set; }
        [BsonElement("Monto")]
        public decimal Monto { get; set; }
        [BsonElement("Metodo")]
        public string Metodo { get; set; }  
    }
}
