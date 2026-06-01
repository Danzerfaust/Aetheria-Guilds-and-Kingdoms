using Microsoft.Data.Sqlite;
using SRGuildsAndKingdoms.src.events;
using System;
using System.Collections.Generic;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.database
{
    public class EventsRepository(ICoreServerAPI serverApi, GuildDatabase database)
    {
        private readonly ICoreServerAPI serverApi = serverApi;
        private readonly GuildDatabase database = database;

        #region Event CRUD

        public int CreateEvent(Event eventData)
        {
            ArgumentNullException.ThrowIfNull(eventData);

            const string sql = @"
                INSERT INTO guild_events (name, description, max_players, start_date, end_date, location_x, location_y, location_z, created_at)
                VALUES (@name, @description, @maxPlayers, @startDate, @endDate, @locationX, @locationY, @locationZ, @createdAt);
                SELECT last_insert_rowid();";

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@name", eventData.Name);
                command.Parameters.AddWithValue("@description", eventData.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@maxPlayers", eventData.MaxPlayers);
                command.Parameters.AddWithValue("@startDate", eventData.StartDate);
                command.Parameters.AddWithValue("@endDate", eventData.EndDate);
                command.Parameters.AddWithValue("@locationX", eventData.LocationX ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@locationY", eventData.LocationY ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@locationZ", eventData.LocationZ ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@createdAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                eventData.Id = Convert.ToInt32(command.ExecuteScalar());
                return eventData.Id;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[EventsRepository] Failed to create event: {ex.Message}");
                throw;
            }
        }

        public bool UpdateEvent(Event eventData)
        {
            ArgumentNullException.ThrowIfNull(eventData);

            const string sql = @"
                UPDATE guild_events
                SET name = @name,
                    description = @description,
                    max_players = @maxPlayers,
                    start_date = @startDate,
                    end_date = @endDate,
                    location_x = @locationX,
                    location_y = @locationY,
                    location_z = @locationZ
                WHERE id = @eventId;";

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@eventId", eventData.Id);
                command.Parameters.AddWithValue("@name", eventData.Name);
                command.Parameters.AddWithValue("@description", eventData.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@maxPlayers", eventData.MaxPlayers);
                command.Parameters.AddWithValue("@startDate", eventData.StartDate);
                command.Parameters.AddWithValue("@endDate", eventData.EndDate);
                command.Parameters.AddWithValue("@locationX", eventData.LocationX ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@locationY", eventData.LocationY ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@locationZ", eventData.LocationZ ?? (object)DBNull.Value);

                int affected = command.ExecuteNonQuery();
                return affected > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[EventsRepository] Failed to update event {eventData.Id}: {ex.Message}");
                throw;
            }
        }

        public bool DeleteEvent(int eventId)
        {
            const string sql = "DELETE FROM guild_events WHERE id = @eventId;";

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@eventId", eventId);

                int affected = command.ExecuteNonQuery();
                return affected > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[EventsRepository] Failed to delete event {eventId}: {ex.Message}");
                throw;
            }
        }

        public List<Event> GetAllEvents()
        {
            const string sql = @"
                SELECT id, name, description, max_players, start_date, end_date, location_x, location_y, location_z, created_at
                FROM guild_events
                ORDER BY start_date DESC;";

            var events = new List<Event>();

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var eventData = ReadEvent(reader);
                    eventData.Registrations = GetEventRegistrations(eventData.Id);
                    events.Add(eventData);
                }

                return events;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[EventsRepository] Failed to get all events: {ex.Message}");
                throw;
            }
        }

        public Event? GetEvent(int eventId)
        {
            const string sql = @"
                SELECT id, name, description, max_players, start_date, end_date, location_x, location_y, location_z, created_at
                FROM guild_events
                WHERE id = @eventId;";

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@eventId", eventId);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var eventData = ReadEvent(reader);
                    eventData.Registrations = GetEventRegistrations(eventId);
                    return eventData;
                }
                return null;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[EventsRepository] Failed to get event {eventId}: {ex.Message}");
                throw;
            }
        }

        public List<Event> GetActiveAndFutureEvents()
        {
            var currentDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            const string sql = @"
                SELECT id, name, description, max_players, start_date, end_date, location_x, location_y, location_z, created_at
                FROM guild_events
                WHERE end_date > @currentDate
                ORDER BY start_date ASC;";

            var events = new List<Event>();

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@currentDate", currentDate);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var eventData = ReadEvent(reader);
                    eventData.Registrations = GetEventRegistrations(eventData.Id);
                    events.Add(eventData);
                }

                return events;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[EventsRepository] Failed to get active and future events: {ex.Message}");
                throw;
            }
        }

        public List<Event> GetFutureEventsForPlayer(string playerUid)
        {
            var currentDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            const string sql = @"
                SELECT e.id, e.name, e.description, e.max_players, e.start_date, e.end_date, e.location_x, e.location_y, e.location_z, e.created_at
                FROM guild_events e
                INNER JOIN guild_event_registrations r ON e.id = r.event_id
                WHERE e.start_date > @currentDate
                  AND r.registree_uid = @playerUid
                ORDER BY e.start_date ASC;";

            var events = new List<Event>();

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@currentDate", currentDate);
                command.Parameters.AddWithValue("@playerUid", playerUid);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    // No need to add registrations here since we're only fetching events the player is registered for
                    events.Add(ReadEvent(reader));
                }

                return events;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[EventsRepository] Failed to get future events for player {playerUid}: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Registrations

        public bool CreateRegistration(int eventId, string playerUid)
        {
            const string sql = @"
                INSERT INTO guild_event_registrations (event_id, registree_uid, registration_date)
                VALUES (@eventId, @registreeUid, @registrationDate);";

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@eventId", eventId);
                command.Parameters.AddWithValue("@registreeUid", playerUid);
                command.Parameters.AddWithValue("@registrationDate", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));

                command.ExecuteNonQuery();
                return true;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return false;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[EventsRepository] Failed to create registration for event {eventId}, player {playerUid}: {ex.Message}");
                throw;
            }
        }

        public bool DeleteRegistration(int eventId, string playerUid)
        {
            const string sql = "DELETE FROM guild_event_registrations WHERE event_id = @eventId AND registree_uid = @registreeUid;";

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@eventId", eventId);
                command.Parameters.AddWithValue("@registreeUid", playerUid);

                int affected = command.ExecuteNonQuery();
                return affected > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[EventsRepository] Failed to delete registration for event {eventId}, player {playerUid}: {ex.Message}");
                throw;
            }
        }

        public List<EventRegistration> GetEventRegistrations(int eventId)
        {
            const string sql = @"
                SELECT event_id, registree_uid, registration_date
                FROM guild_event_registrations
                WHERE event_id = @eventId
                ORDER BY registration_date ASC;";

            var registrations = new List<EventRegistration>();

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@eventId", eventId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    registrations.Add(ReadEventRegistration(reader));
                }

                return registrations;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[EventsRepository] Failed to get registrations for event {eventId}: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Helper Methods

        private Event ReadEvent(SqliteDataReader reader)
        {
            return new Event
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                MaxPlayers = reader.GetInt32(3),
                StartDate = reader.GetString(4),
                EndDate = reader.GetString(5),
                LocationX = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                LocationY = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                LocationZ = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                CreatedAt = reader.GetInt64(9),
                Registrations = []
            };
        }

        private EventRegistration ReadEventRegistration(SqliteDataReader reader)
        {
            return new EventRegistration
            {
                EventId = reader.GetInt32(0),
                RegistreeUid = reader.GetString(1),
                RegistrationDate = reader.GetString(2)
            };
        }

        #endregion
    }
}
