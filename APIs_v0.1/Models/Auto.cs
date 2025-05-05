using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace APIs_v0._1.Models
{
    public class Auto
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }


        [BsonElement("Modelo")]
        public string Modelo { get; set; }

        [BsonElement("FechaModelo")]

        public required string FechaModelo { get; set; }

        [BsonElement("Costo")]
        public decimal Costo { get; set; }


        [BsonElement("Fabricante")]
        public string Fabricante { get; set; }

        [BsonElement("NumeroSerie")]
        public string NumeroSerie { get; set; }

        //[BsonElement("Accesorios")]
        //public List<Accesorio> Accesorios { get; set; } = new List<Accesorio>();
    }
}
