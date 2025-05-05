using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace APIs_v0._1.Models
{
    public class Venta
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("IdCotizacion")]
        public int IdCotizacion { get; set; }

        [BsonElement("IdAuto")]
        public string IdAuto { get; set; }   
        
        [BsonElement("IdCliente")]
        [BsonRequired]
        public  int IdCliente { get; set; }

        [BsonElement("IdSeguro")]
        [BsonRequired]
        public int? IdSeguro { get; set; }

        [BsonElement("Fecha")]
        public DateTime Fecha { get; set; }

        [BsonElement("VentaAuto")]
        public VentaAuto VentaAuto { get; set; }

        [BsonElement("VentaSeguro")]
        public VentaSeguro VentaSeguro { get; set; }
    }
}
