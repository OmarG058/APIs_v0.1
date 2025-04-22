using MongoDB.Bson.Serialization.Attributes;

namespace APIs_v0._1.Models
{
    public class Accesorio
    {
        [BsonElement("IdAccesorio")]
        public int IdAccesorio { get; set; }

        [BsonElement("Nombre")]
        public string Nombre { get; set; }

        [BsonElement("Tipo")]
        public string Tipo { get; set; }
        [BsonElement("Costo")]
        public decimal Costo { get; set; }
        //posiblemente agregar un campo de precio

    }
}
