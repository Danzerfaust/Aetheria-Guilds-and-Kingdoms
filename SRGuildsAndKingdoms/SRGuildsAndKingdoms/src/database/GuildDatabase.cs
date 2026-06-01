using Microsoft.Data.Sqlite;
using System;
using System.IO;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.database
{
    /// <summary>
    /// Manages the SQLite database connection and configuration for guild data
    /// Uses WAL (Write-Ahead Logging) mode for better concurrency and performance
    /// </summary>
    public class GuildDatabase : IDisposable
    {
        private readonly ICoreServerAPI serverApi;
        private readonly string databasePath;
        private SqliteConnection? connection;
        private readonly DatabaseSchema schema;
        private bool isDisposed = false;

        /// <summary>
        /// Gets the active database connection
        /// </summary>
        public SqliteConnection Connection
        {
            get
            {
                if (connection == null || connection.State != System.Data.ConnectionState.Open)
                {
                    throw new InvalidOperationException("Database connection is not open. Call Initialize() first.");
                }
                return connection;
            }
        }

        /// <summary>
        /// Gets whether the database is currently connected and ready
        /// </summary>
        public bool IsConnected => connection != null && connection.State == System.Data.ConnectionState.Open;

        public GuildDatabase(ICoreServerAPI serverApi)
        {
            this.serverApi = serverApi;

            // Store database in ModData folder
            var dataFolder = serverApi.GetOrCreateDataPath("ModData/SRGuildsAndKingdoms");
            databasePath = Path.Combine(dataFolder, "guilds.db");

            schema = new DatabaseSchema(serverApi);
        }

        /// <summary>
        /// Initializes the database connection and schema
        /// </summary>
        public void Initialize()
        {
            try
            {
                serverApi.Logger.Notification($"[SRGuildsAndKingdoms] Initializing database at: {databasePath}");

                var connectionStringBuilder = new SqliteConnectionStringBuilder
                {
                    DataSource = databasePath,
                    Mode = SqliteOpenMode.ReadWriteCreate,
                    Cache = SqliteCacheMode.Shared,
                    Pooling = true
                };

                connection = new SqliteConnection(connectionStringBuilder.ToString());
                connection.Open();

                EnableWalMode();
                ConfigureConnection();

                schema.InitializeSchema(connection);

                // Verify schema is correct
                if (!schema.VerifySchema(connection))
                {
                    throw new InvalidOperationException("Database schema verification failed");
                }

                serverApi.Logger.Notification("[SRGuildsAndKingdoms] Database initialized successfully");
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms] Failed to initialize database: {ex.Message}");
                serverApi.Logger.Error($"[SRGuildsAndKingdoms] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Enables WAL
        /// </summary>
        private void EnableWalMode()
        {
            try
            {
                using var command = connection!.CreateCommand();

                command.CommandText = "PRAGMA journal_mode = WAL;";
                var result = command.ExecuteScalar()?.ToString();

                command.CommandText = "PRAGMA synchronous = NORMAL;";
                command.ExecuteNonQuery();

                command.CommandText = "PRAGMA wal_autocheckpoint = 1000;";
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                serverApi.Logger.Warning($"[SRGuildsAndKingdoms] Failed to enable WAL mode: {ex.Message}");
            }
        }

        /// <summary>
        /// SQLite connection
        /// </summary>
        private void ConfigureConnection()
        {
            try
            {
                using var command = connection!.CreateCommand();
                command.CommandText = "PRAGMA page_size = 4096;";
                command.ExecuteNonQuery();

                command.CommandText = "PRAGMA cache_size = -10000;";
                command.ExecuteNonQuery();

                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();

                command.CommandText = "PRAGMA temp_store = MEMORY;";
                command.ExecuteNonQuery();

                command.CommandText = "PRAGMA mmap_size = 30000000;";
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                serverApi.Logger.Warning($"[SRGuildsAndKingdoms] Failed to apply some optimizations: {ex.Message}");
            }
        }

        /// <summary>
        /// Performs a WAL checkpoint to commit all pending changes
        /// </summary>
        public void Checkpoint()
        {
            try
            {
                if (!IsConnected) return;

                using var command = connection!.CreateCommand();
                command.CommandText = "PRAGMA wal_checkpoint(PASSIVE);";
                var result = command.ExecuteScalar();

                serverApi.Logger.Debug($"[SRGuildsAndKingdoms] Checkpoint performed: {result}");
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms] Checkpoint failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a db backup
        /// </summary>
        public void CreateBackup(string backupPath)
        {
            try
            {
                if (!IsConnected)
                {
                    throw new InvalidOperationException("Cannot create backup: database is not connected");
                }

                serverApi.Logger.Notification($"[SRGuildsAndKingdoms] Creating backup at: {backupPath}");

                var backupDir = Path.GetDirectoryName(backupPath);
                if (!string.IsNullOrEmpty(backupDir) && !Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                // Perform checkpoint first
                Checkpoint();

                using var destination = new SqliteConnection($"Data Source={backupPath}");
                destination.Open();
                connection!.BackupDatabase(destination);

                serverApi.Logger.Notification("[SRGuildsAndKingdoms] Backup created successfully");
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms] Backup failed: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            if (isDisposed) return;

            try
            {
                if (connection != null)
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        // Final checkpoint before closing
                        Checkpoint();
                        connection.Close();
                    }
                    connection.Dispose();
                    connection = null;
                }

                serverApi.Logger.Debug("[SRGuildsAndKingdoms] Database connection disposed");
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms] Error disposing database: {ex.Message}");
            }
            finally
            {
                isDisposed = true;
            }
        }
    }
}
