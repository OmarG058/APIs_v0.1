using MongoDB.Driver;
using APIs_v0._1.Models;

namespace APIs_v0._1.Services
{
    public class MongoDbService
    {
        private readonly IMongoCollection<Auto> _AutosCollection;
        private IMongoCollection <Venta> _VentasCollection;

        // private readonly IMongoCollection<Ventas> _VentasCollection;
        public MongoDbService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoSettings:ConnectionString"]);
            var database = client.GetDatabase(config["MongoSettings:DatabaseName"]);
            _AutosCollection = database.GetCollection<Auto>(config["MongoSettings:CollectionName"]);
           _VentasCollection = database.GetCollection<Venta>(config["MongoSettings:CollectionName2"]);
        }
        //------------------------------------Autos---------------------------------
        //obtener los autos por id
        public async Task<List<Auto>> GetAutosById(string idAuto) => await _AutosCollection.Find(A => A.Id == idAuto).ToListAsync();
        //obtener todos los autos 
        public async Task<List<Auto>> GetAllAutos() => await _AutosCollection.Find(_ => true).ToListAsync();
        //insertar en autos 
        public async Task AgregarAuto(Auto Auto) => await _AutosCollection.InsertOneAsync(Auto);

        //---------------------------------Ventas---------------------------------
        public async Task<List<Venta>> GetVentas() => await _VentasCollection.Find(_ =>true).ToListAsync();
        public async Task<List<Venta>> GetVentasById (string id) => await _VentasCollection.Find(v => v.Id == id).ToListAsync();
        public async Task<List<Venta>> GetVentasByCliente(int? idCliente) => await _VentasCollection.Find(v => v.IdCliente == idCliente).ToListAsync();
        public async Task<List<Venta>> GetVentasByFecha(DateTime fecha)
        {
            var fechaInicio = DateTime.SpecifyKind(fecha.Date, DateTimeKind.Utc);
            var fechaFin = fechaInicio.AddDays(1);

            Console.WriteLine($"Filtro entre: {fechaInicio:o} y {fechaFin:o}");

            var filter = Builders<Venta>.Filter.Gte(v => v.Fecha, fechaInicio) &
                         Builders<Venta>.Filter.Lt(v => v.Fecha, fechaFin);

            var ventas = await _VentasCollection.Find(filter).ToListAsync();

            Console.WriteLine($"Ventas encontradas: {ventas.Count}");

            return ventas;
        }
        public async Task AgregarVenta(Venta venta) => await _VentasCollection.InsertOneAsync(venta);
        public async Task ActualizarVenta(string id, Venta venta)
        {
            var filter = Builders<Venta>.Filter.Eq(v => v.Id, id);
            await _VentasCollection.ReplaceOneAsync(filter, venta);
        }

    }
}
