using ECommerceMaui.Models;
using MySql.Data.MySqlClient; // ¡El paquete que instalamos!
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
            "SslMode=Required"; // Necesario para conexiones locales sin SSL

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
                        Stock = reader.GetInt32("stock")
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
        public async Task<bool> RegisterUserAsync(string email, string fullName, string passwordHash)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                // Comando SQL para insertar
                var command = new MySqlCommand(
                    "INSERT INTO users (email, full_name, password_hash) VALUES (@Email, @FullName, @PasswordHash)",
                    connection);

                // Usamos parámetros para evitar Inyección SQL
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@FullName", fullName);
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);

                // Ejecutamos el comando
                int rowsAffected = await command.ExecuteNonQueryAsync();

                // Si se afectó una fila (se insertó 1 usuario), retorna true
                return rowsAffected > 0;
            }
            catch (MySqlException ex)
            {
                // El código 1062 es "Entrada duplicada" (email ya existe)
                if (ex.Number == 1062)
                {
                    Debug.WriteLine("Error de registro: El email ya existe.");
                }
                else
                {
                    Debug.WriteLine($"Error de MySQL al registrar: {ex.Message}");
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error general al registrar: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifica las credenciales Y devuelve los datos del usuario si es exitoso.
        /// </summary>
        /// <returns>Un objeto 'User' si el login es exitoso, 'null' si no.</returns>
        public async Task<User> LoginUserAsync(string email, string plainPassword)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // 1. Buscamos al usuario por su email y traemos su info
                    var command = new MySqlCommand(
                        "SELECT user_id, full_name, password_hash FROM users WHERE email = @Email",
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
                                // ¡Contraseña correcta! Creamos y devolvemos el objeto User
                                var user = new User
                                {
                                    UserId = reader.GetInt32("user_id"),
                                    Email = email, // Ya lo tenemos
                                    FullName = reader.GetString("full_name")
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

        public async Task<bool> UpdateStockAsync(ObservableCollection<Product> cartItems)
        {
            // 1. Convertir la lista del carrito [Laptop, Smart, Laptop]
            //    en un diccionario de {ID_Producto, Cantidad} -> { {1, 2}, {2, 1} }
            var productQuantities = new Dictionary<int, int>();
            foreach (var product in cartItems)
            {
                if (productQuantities.ContainsKey(product.Id))
                {
                    productQuantities[product.Id]++;
                }
                else
                {
                    productQuantities[product.Id] = 1;
                }
            }

            // 2. Abrir conexión y empezar una transacción
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // 3. Crear el comando (reutilizable)
                        var command = new MySqlCommand(
                            "UPDATE products SET stock = stock - @Quantity " +
                            "WHERE product_id = @ProductId AND stock >= @Quantity",
                            connection, transaction);

                        // 4. Iterar sobre cada producto en nuestro diccionario
                        foreach (var item in productQuantities)
                        {
                            var productId = item.Key;
                            var quantityToBuy = item.Value;

                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@Quantity", quantityToBuy);
                            command.Parameters.AddWithValue("@ProductId", productId);

                            // 5. Ejecutar la actualización
                            int rowsAffected = await command.ExecuteNonQueryAsync();

                            // 6. ¡Verificación clave!
                            // Si rowsAffected es 0, significa que la condición "AND stock >= @Quantity"
                            // falló (no hay suficiente stock).
                            if (rowsAffected == 0)
                            {
                                await transaction.RollbackAsync(); // ¡Deshacer todo!
                                Debug.WriteLine($"Error de stock para Producto ID: {productId}");
                                return false; // Indicar fallo
                            }
                        }

                        // 7. Si el bucle termina sin problemas, todo está bien.
                        await transaction.CommitAsync(); // ¡Confirmar todos los cambios!
                        return true; // Indicar éxito
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error en transacción de stock: {ex.Message}");
                        await transaction.RollbackAsync(); // Deshacer por si acaso
                        return false;
                    }
                }
            }
        }
    }
}
