using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

#nullable disable

namespace MetaQuestTrayManager.Data
{
    /// <summary>
    /// Manages database operations for MetaQuestTrayManager using SQLite.
    /// </summary>
    public static class DatabaseManager
    {
        private static SQLiteConnection _dbConnection;
        private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MetaQuest.db");

        /// <summary>
        /// Opens or initializes the database connection.
        /// </summary>
        public static void OpenDatabase()
        {
            try
            {
                if (!File.Exists(DbPath))
                {
                    Console.WriteLine("Database not found, creating a new one...");
                    InitializeDatabase();
                }

                if (_dbConnection == null || _dbConnection.State == ConnectionState.Closed)
                {
                    _dbConnection = new SQLiteConnection($"Data Source={DbPath}; Version=3;");
                    _dbConnection.Open();
                    Console.WriteLine("Database connection established.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening database: {ex.Message}");
            }
        }

        /// <summary>
        /// Initializes the database by creating required tables.
        /// </summary>
        private static void InitializeDatabase()
        {
            try
            {
                _dbConnection = new SQLiteConnection($"Data Source={DbPath}; Version=3;");
                _dbConnection.Open();

                var tables = new List<string>
                {
                    "CREATE TABLE IF NOT EXISTS profiles (ID INTEGER PRIMARY KEY AUTOINCREMENT, DisplayName TEXT, ASW TEXT DEFAULT 'Inherit', Priority TEXT DEFAULT 'Normal', Enabled TEXT DEFAULT 'Yes')",
                    "CREATE TABLE IF NOT EXISTS knownApps (ID INTEGER PRIMARY KEY AUTOINCREMENT, FileName TEXT, DisplayName TEXT, CompletePath TEXT)",
                    "CREATE TABLE IF NOT EXISTS hiddenApps (ID INTEGER PRIMARY KEY AUTOINCREMENT, DisplayName TEXT, LaunchFile TEXT, Location TEXT)",
                    "CREATE TABLE IF NOT EXISTS ignoredApps (ID INTEGER PRIMARY KEY AUTOINCREMENT, FileName TEXT)"
                };

                foreach (var tableCommand in tables)
                {
                    ExecuteNonQuery(tableCommand);
                }

                Console.WriteLine("Database initialized with required tables.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database: {ex.Message}");
            }
        }

        /// <summary>
        /// Executes a non-query SQL command.
        /// </summary>
        /// <param name="commandText">The SQL query to execute.</param>
        private static void ExecuteNonQuery(string commandText)
        {
            using var command = new SQLiteCommand(commandText, _dbConnection);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Adds or updates a profile in the database.
        /// </summary>
        public static void AddOrUpdateProfile(string displayName, string asw, string priority, string enabled)
        {
            try
            {
                var commandText = @"
                    INSERT OR REPLACE INTO profiles (DisplayName, ASW, Priority, Enabled)
                    VALUES (@DisplayName, @ASW, @Priority, @Enabled)";

                using var command = new SQLiteCommand(commandText, _dbConnection);
                command.Parameters.AddWithValue("@DisplayName", displayName);
                command.Parameters.AddWithValue("@ASW", asw);
                command.Parameters.AddWithValue("@Priority", priority);
                command.Parameters.AddWithValue("@Enabled", enabled);

                command.ExecuteNonQuery();
                Console.WriteLine($"Profile '{displayName}' added or updated.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding/updating profile: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all profiles from the database.
        /// </summary>
        public static List<string> GetAllProfiles()
        {
            var profiles = new List<string>();
            try
            {
                const string query = "SELECT DisplayName FROM profiles";
                using var command = new SQLiteCommand(query, _dbConnection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    profiles.Add(reader.GetString(0));
                }

                Console.WriteLine($"{profiles.Count} profiles retrieved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving profiles: {ex.Message}");
            }

            return profiles;
        }

        /// <summary>
        /// Deletes a profile by name.
        /// </summary>
        public static void RemoveProfile(string displayName)
        {
            try
            {
                var commandText = "DELETE FROM profiles WHERE DisplayName = @DisplayName";
                using var command = new SQLiteCommand(commandText, _dbConnection);
                command.Parameters.AddWithValue("@DisplayName", displayName);
                command.ExecuteNonQuery();

                Console.WriteLine($"Profile '{displayName}' removed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing profile: {ex.Message}");
            }
        }

        /// <summary>
        /// Closes the database connection.
        /// </summary>
        public static void CloseDatabase()
        {
            if (_dbConnection?.State != ConnectionState.Closed)
            {
                _dbConnection.Close();
                Console.WriteLine("Database connection closed.");
            }
        }
    }
}
