using ECommerceMaui.Models;
using MySql.Data.MySqlClient; 
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics; // Para ver errores en la consola

namespace ECommerceMaui.Servicios
{
    public class DatabaseService
    {
        // --- 1. Cadena de Conexión ---
        // encontrar base de datos MySQL.
        private readonly string _connectionString =
            "Server=ba1gi7jlgly7zrbpnsax-mysql.services.clever-cloud.com;" +
            "Database=ba1gi7jlgly7zrbpnsax;" +
            "User=uf14aujm4gsrloba;" +
            "Password=UP8DB6VBmnAm4QdusGBA;" +
            "SslMode=Required;" + // Necesario para conexiones locales sin SSL
            "Pooling=false;";

         /// <summary>
        /// Obtiene todos los productos de la base de datos.
        /// </summary>
        public async Task<ObservableCollection<Product>> GetProductsAsync()
        {
            var products = new ObservableCollection<Product>();

            try
            {
                // 'using' asegura que la conexión se cierre automáticamente
                using var connection = new MySqlConnection(_connectionString);
                // 1. Abrimos la conexión
                await connection.OpenAsync();

                // 2. Creamos el comando SQL
                var command = new MySqlCommand("SELECT * FROM products", connection);

                // 3. Ejecutamos el comando y obtenemos un "lector"
                using var reader = await command.ExecuteReaderAsync();
                // 4. Leemos los resultados fila por fila
                while (await reader.ReadAsync())
                {
                    var product = new Product
                    {
                        // Leemos cada columna por su nombre
                        Id = reader.GetInt32("product_id"),
                        Name = reader.GetString("name"),
                        Description = reader.GetString("description"),
                        Price = reader.GetDouble("price"), // C# usa double, SQL usa decimal
                        ImageUrl = reader.GetString("image_url"),
                        Stock = reader.GetInt32("stock"),
                        CategoryId = reader.IsDBNull(reader.GetOrdinal("category_id")) ? 0 : reader.GetInt32("category_id")
                    };
                    products.Add(product);
                }
            }
            catch (MySqlException ex)
            {
                // Si algo sale mal (ej. contraseña incorrecta), lo veremos en la consola
                Debug.WriteLine($"Error de MySQL: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Cualquier otro error
                Debug.WriteLine($"Error general: {ex.Message}");
            }

            return products;
        }

        /// <summary>
        /// Registra un nuevo usuario en la base de datos.
        /// </summary>
        /// <returns>True si fue exitoso, False si falló (ej. email ya existe).</returns>
        public async Task<bool> RegisterUserAsync(string email, string fullName, string passwordHash, int avatarId)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var command = new MySqlCommand(
                        "INSERT INTO users (email, full_name, password_hash, avatar_id) VALUES (@Email, @FullName, @PasswordHash, @AvatarId)",
                        connection);

                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@FullName", fullName);
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    command.Parameters.AddWithValue("@AvatarId", avatarId); // <-- Nuevo

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al registrar: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifica las credenciales Y devuelve los datos del usuario si es exitoso.
        /// </summary>
        
        
        public async Task<User> LoginUserAsync(string email, string plainPassword)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // 1. Buscamos al usuario por su email y traemos su info
                    var command = new MySqlCommand(
                            "SELECT user_id, full_name, password_hash, avatar_id FROM users WHERE email = @Email",
                            connection);
                    command.Parameters.AddWithValue("@Email", email);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // 2. Usuario encontrado, obtenemos el hash guardado
                            var storedHash = reader.GetString("password_hash");

                            // 3. Ciframos la contraseña proporcionada
                            var providedHash = Helpers.PasswordHelper.HashPassword(plainPassword);

                            // 4. Comparamos los hashes
                            if (storedHash == providedHash)
                            {
                                var user = new User
                                {
                                    UserId = reader.GetInt32("user_id"),
                                    Email = email,
                                    FullName = reader.GetString("full_name"),
                                    AvatarId = reader.GetInt32("avatar_id") // <-- Nuevo: Leemos el avatar
                                };
                                return user;
                         }
                      }
                    }

                    // Si llegamos aquí, es porque el usuario no se encontró o la contraseña fue incorrecta
                    Debug.WriteLine("Error de login: Email o contraseña incorrectos.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error general en login: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Elimina un usuario de la base de datos usando su ID.
        /// </summary>
        /// <returns>True si fue exitoso, False si falló.</returns>
        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Comando SQL para eliminar
                    var command = new MySqlCommand(
                        "DELETE FROM users WHERE user_id = @UserId",
                        connection);

                    command.Parameters.AddWithValue("@UserId", userId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    // Si se afectó una fila (se eliminó 1 usuario), retorna true
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error general al eliminar usuario: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Registra que un usuario ha visto un producto.
        /// </summary>
        public async Task LogProductViewAsync(int userId, int productId)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Comando para insertar en el historial
                    // (No nos importa si falla, así que no necesitamos transacciones)
                    var command = new MySqlCommand(
                        "INSERT INTO view_history (user_id, product_id) VALUES (@UserId, @ProductId)",
                        connection);

                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@ProductId", productId);

                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                // Fallo silencioso. No queremos molestar al usuario si
                // el registro de historial falla. Solo lo mostramos en depuración.
                System.Diagnostics.Debug.WriteLine($"Error al registrar vista: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene todas las reseñas de un producto específico.
        /// (Usa JOIN para obtener el nombre del autor desde la tabla 'users')
        /// </summary>
        public async Task<ObservableCollection<Review>> GetReviewsForProductAsync(int productId)
        {
            var reviews = new ObservableCollection<Review>();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var command = new MySqlCommand(
                        "SELECT r.review_id, r.product_id, r.rating, r.comment, r.created_at, u.full_name " +
                        "FROM reviews r " +
                        "JOIN users u ON r.user_id = u.user_id " +
                        "WHERE r.product_id = @ProductId " +
                        "ORDER BY r.created_at DESC",
                        connection);

                    command.Parameters.AddWithValue("@ProductId", productId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            reviews.Add(new Review
                            {
                                ReviewId = reader.GetInt32("review_id"),
                                ProductId = reader.GetInt32("product_id"),
                                Rating = reader.GetInt32("rating"),
                                Comment = reader.GetString("comment"),
                                CreatedAt = reader.GetDateTime("created_at"),
                                AuthorName = reader.GetString("full_name")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener reseñas: {ex.Message}");
            }
            return reviews;
        }

        /// <summary>
        /// Publica una nueva reseña en la base de datos.
        /// </summary>
        public async Task<bool> SubmitReviewAsync(int productId, int userId, int rating, string comment)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var command = new MySqlCommand(
                        "INSERT INTO reviews (product_id, user_id, rating, comment) " +
                        "VALUES (@ProductId, @UserId, @Rating, @Comment)",
                        connection);

                    command.Parameters.AddWithValue("@ProductId", productId);
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Rating", rating);
                    command.Parameters.AddWithValue("@Comment", comment);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al enviar reseña: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Procesa la compra completa: Crea la orden, detalles y actualiza inventario.
        /// Todo dentro de una transacción atómica.
        /// </summary>
        public async Task<bool> ProcessOrderAsync(ObservableCollection<Product> cartItems, int userId, double totalAmount)
        {
            // 1. Agrupar productos por ID para manejar cantidades (si compró 2 laptops iguales)
            var productQuantities = cartItems
                .GroupBy(p => p.Id)
                .ToDictionary(g => g.Key, g => g.Count());

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // ---------------------------------------------------------
                        // PASO A: Crear la Orden (Header)
                        // ---------------------------------------------------------
                        var orderCommand = new MySqlCommand(
                            "INSERT INTO orders (user_id, total_amount) VALUES (@UserId, @Total); SELECT LAST_INSERT_ID();",
                            connection, transaction);

                        orderCommand.Parameters.AddWithValue("@UserId", userId);
                        orderCommand.Parameters.AddWithValue("@Total", totalAmount);

                        // Ejecutamos y obtenemos el ID generado automáticamente (order_id)
                        var newOrderIdLong = await orderCommand.ExecuteScalarAsync();
                        int newOrderId = Convert.ToInt32(newOrderIdLong);

                        // ---------------------------------------------------------
                        // PASO B: Procesar cada producto (Stock y Detalles)
                        // ---------------------------------------------------------
                        foreach (var item in productQuantities)
                        {
                            int productId = item.Key;
                            int quantity = item.Value;

                            // Obtenemos el precio unitario del producto original en la lista
                            // (Tomamos el primero del grupo para obtener el precio)
                            double price = cartItems.First(p => p.Id == productId).Price;

                            // 1. Actualizar Stock (Verificar que haya suficiente)
                            var stockCommand = new MySqlCommand(
                                "UPDATE products SET stock = stock - @Quantity WHERE product_id = @ProductId AND stock >= @Quantity",
                                connection, transaction);
                            stockCommand.Parameters.AddWithValue("@Quantity", quantity);
                            stockCommand.Parameters.AddWithValue("@ProductId", productId);

                            int rowsAffected = await stockCommand.ExecuteNonQueryAsync();

                            if (rowsAffected == 0)
                            {
                                // Si no afectó filas, es que no había stock suficiente
                                await transaction.RollbackAsync();
                                Debug.WriteLine($"Stock insuficiente para producto ID: {productId}");
                                return false;
                            }

                            // 2. Insertar Detalle de Orden
                            var detailCommand = new MySqlCommand(
                                "INSERT INTO order_details (order_id, product_id, quantity, price_per_unit) VALUES (@OrderId, @ProductId, @Quantity, @Price)",
                                connection, transaction);
                            detailCommand.Parameters.AddWithValue("@OrderId", newOrderId);
                            detailCommand.Parameters.AddWithValue("@ProductId", productId);
                            detailCommand.Parameters.AddWithValue("@Quantity", quantity);
                            detailCommand.Parameters.AddWithValue("@Price", price);

                            await detailCommand.ExecuteNonQueryAsync();
                        }

                        // ---------------------------------------------------------
                        // PASO C: Confirmar todo
                        // ---------------------------------------------------------
                        await transaction.CommitAsync();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error en transacción de compra: {ex.Message}");
                        await transaction.RollbackAsync();
                        return false;
                    }
                }
            }
        }
        public async Task<List<Category>> GetCategoriesAsync()
        {
            var categories = new List<Category>();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var command = new MySqlCommand("SELECT * FROM categories", connection);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            categories.Add(new Category
                            {
                                Id = reader.GetInt32("category_id"),
                                Name = reader.GetString("name"),
                                Description = reader.GetString("description")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error obteniendo categorías: {ex.Message}");
            }
            return categories;
        }
    }
}
