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
        //PARA MONGODB LA LISTA DE AUTOS
        // Obtener todos los autos
        public async Task<List<Auto>> GetAllAutos() =>
            await _AutosCollection.Find(_ => true).ToListAsync();

        // Obtener un auto por ID
        public async Task<Auto> GetAutosById(string id) =>
            await _AutosCollection.Find(A => A.Id == id).FirstOrDefaultAsync();

        // Agregar un auto nuevo
        public async Task AgregarAuto(Auto auto) =>
            await _AutosCollection.InsertOneAsync(auto);

        // Actualizar un auto
        public async Task<bool> ActualizarAuto(string id, Auto auto)
        {
            var result = await _AutosCollection.ReplaceOneAsync(A => A.Id == id, auto);
            return result.ModifiedCount > 0;
        }

        // Eliminar un auto
        public async Task<bool> EliminarAuto(string id)
        {
            var result = await _AutosCollection.DeleteOneAsync(A => A.Id == id);
            return result.DeletedCount > 0;
        }

        // Buscar autos por Modelo
        public async Task<List<Auto>> GetAutosByModelo(string modelo) =>
            await _AutosCollection.Find(auto => auto.Modelo.ToLower() == modelo.ToLower()).ToListAsync();

        // Buscar autos por Fabricante
        public async Task<List<Auto>> GetAutosByFabricante(string fabricante) =>
            await _AutosCollection.Find(auto => auto.Fabricante.ToLower() == fabricante.ToLower()).ToListAsync();


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
        public async Task<DeleteResult> EliminarVenta(string id) => await _VentasCollection.DeleteOneAsync(V=>V.Id == id);

    }
}
