using System;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.database
{
	// Token: 0x020000B3 RID: 179
	[NullableContext(1)]
	[Nullable(0)]
	public class GuildDatabase : IDisposable
	{
		// Token: 0x17000220 RID: 544
		// (get) Token: 0x0600082A RID: 2090 RVA: 0x000397AC File Offset: 0x000379AC
		public SqliteConnection Connection
		{
			get
			{
				if (this.connection == null || this.connection.State != ConnectionState.Open)
				{
					throw new InvalidOperationException("Database connection is not open. Call Initialize() first.");
				}
				return this.connection;
			}
		}

		// Token: 0x17000221 RID: 545
		// (get) Token: 0x0600082B RID: 2091 RVA: 0x000397D5 File Offset: 0x000379D5
		public bool IsConnected
		{
			get
			{
				return this.connection != null && this.connection.State == ConnectionState.Open;
			}
		}

		// Token: 0x0600082C RID: 2092 RVA: 0x000397F0 File Offset: 0x000379F0
		public GuildDatabase(ICoreServerAPI serverApi)
		{
			this.serverApi = serverApi;
			string dataFolder = serverApi.GetOrCreateDataPath("ModData/SRGuildsAndKingdoms");
			this.databasePath = Path.Combine(dataFolder, "guilds.db");
			this.schema = new DatabaseSchema(serverApi);
		}

		// Token: 0x0600082D RID: 2093 RVA: 0x00039834 File Offset: 0x00037A34
		public void Initialize()
		{
			try
			{
				this.serverApi.Logger.Notification("[SRGuildsAndKingdoms] Initializing database at: " + this.databasePath);
				SqliteConnectionStringBuilder connectionStringBuilder = new SqliteConnectionStringBuilder
				{
					DataSource = this.databasePath,
					Mode = 0,
					Cache = 2,
					Pooling = true
				};
				this.connection = new SqliteConnection(connectionStringBuilder.ToString());
				this.connection.Open();
				this.EnableWalMode();
				this.ConfigureConnection();
				this.schema.InitializeSchema(this.connection);
				if (!this.schema.VerifySchema(this.connection))
				{
					throw new InvalidOperationException("Database schema verification failed");
				}
				this.serverApi.Logger.Notification("[SRGuildsAndKingdoms] Database initialized successfully");
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms] Failed to initialize database: " + ex.Message);
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms] Stack trace: " + ex.StackTrace);
				throw;
			}
		}

		// Token: 0x0600082E RID: 2094 RVA: 0x0003994C File Offset: 0x00037B4C
		private void EnableWalMode()
		{
			try
			{
				using (SqliteCommand command = this.connection.CreateCommand())
				{
					command.CommandText = "PRAGMA journal_mode = WAL;";
					object obj = command.ExecuteScalar();
					if (obj != null)
					{
						obj.ToString();
					}
					command.CommandText = "PRAGMA synchronous = NORMAL;";
					command.ExecuteNonQuery();
					command.CommandText = "PRAGMA wal_autocheckpoint = 1000;";
					command.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Warning("[SRGuildsAndKingdoms] Failed to enable WAL mode: " + ex.Message);
			}
		}

		// Token: 0x0600082F RID: 2095 RVA: 0x000399F4 File Offset: 0x00037BF4
		private void ConfigureConnection()
		{
			try
			{
				using (SqliteCommand command = this.connection.CreateCommand())
				{
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
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Warning("[SRGuildsAndKingdoms] Failed to apply some optimizations: " + ex.Message);
			}
		}

		// Token: 0x06000830 RID: 2096 RVA: 0x00039AB4 File Offset: 0x00037CB4
		public void Checkpoint()
		{
			try
			{
				if (this.IsConnected)
				{
					using (SqliteCommand command = this.connection.CreateCommand())
					{
						command.CommandText = "PRAGMA wal_checkpoint(PASSIVE);";
						object result = command.ExecuteScalar();
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(44, 1);
						defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms] Checkpoint performed: ");
						defaultInterpolatedStringHandler.AppendFormatted<object>(result);
						logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms] Checkpoint failed: " + ex.Message);
			}
		}

		// Token: 0x06000831 RID: 2097 RVA: 0x00039B70 File Offset: 0x00037D70
		public void CreateBackup(string backupPath)
		{
			try
			{
				if (!this.IsConnected)
				{
					throw new InvalidOperationException("Cannot create backup: database is not connected");
				}
				this.serverApi.Logger.Notification("[SRGuildsAndKingdoms] Creating backup at: " + backupPath);
				string backupDir = Path.GetDirectoryName(backupPath);
				if (!string.IsNullOrEmpty(backupDir) && !Directory.Exists(backupDir))
				{
					Directory.CreateDirectory(backupDir);
				}
				this.Checkpoint();
				using (SqliteConnection destination = new SqliteConnection("Data Source=" + backupPath))
				{
					destination.Open();
					this.connection.BackupDatabase(destination);
					this.serverApi.Logger.Notification("[SRGuildsAndKingdoms] Backup created successfully");
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms] Backup failed: " + ex.Message);
				throw;
			}
		}

		// Token: 0x06000832 RID: 2098 RVA: 0x00039C54 File Offset: 0x00037E54
		public void Dispose()
		{
			if (this.isDisposed)
			{
				return;
			}
			try
			{
				if (this.connection != null)
				{
					if (this.connection.State == ConnectionState.Open)
					{
						this.Checkpoint();
						this.connection.Close();
					}
					this.connection.Dispose();
					this.connection = null;
				}
				this.serverApi.Logger.Debug("[SRGuildsAndKingdoms] Database connection disposed");
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms] Error disposing database: " + ex.Message);
			}
			finally
			{
				this.isDisposed = true;
			}
		}

		// Token: 0x04000358 RID: 856
		private readonly ICoreServerAPI serverApi;

		// Token: 0x04000359 RID: 857
		private readonly string databasePath;

		// Token: 0x0400035A RID: 858
		[Nullable(2)]
		private SqliteConnection connection;

		// Token: 0x0400035B RID: 859
		private readonly DatabaseSchema schema;

		// Token: 0x0400035C RID: 860
		private bool isDisposed;
	}
}
