using System.Collections.Generic;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Configuration options for the TfsTeamSettingsProcessor, which handles migration of team configurations, capacities, and team-specific settings.
    /// </summary>
    public class TfsTeamSettingsProcessorOptions : ProcessorOptions
    {
        /// <summary>
        /// Migrate original team settings after their creation on target team project
        /// </summary>
        /// <default>false</default>
        public bool MigrateTeamSettings { get; set; }

        /// <summary>
        /// Reset the target team settings to match the source if the team exists
        /// </summary>
        /// <default>false</default>
        public bool UpdateTeamSettings { get; set; }

        /// <summary>
        /// Prefix your iterations and areas with the project name. If you have enabled this in `NodeStructuresMigrationConfig` you must do it here too.
        /// </summary>
        /// <default>false</default>
        public bool PrefixProjectToNodes { get; set; }

        /// <summary>
        /// Migrate original team member capacities after their creation on the target team project. Note: It will only migrate team member capacity if the team member with same display name exists on the target collection otherwise it will be ignored.
        /// </summary>
        /// <default>false</default>
        public bool MigrateTeamCapacities { get; set; }

        /// <summary>
        /// List of Teams to process. If this is `null` then all teams will be processed.
        /// </summary>
        public List<string> Teams { get; set; }

        /// <summary>
        /// Use user mapping file from TfsTeamSettingsTool when matching users when migrating capacities.
        /// By default, users in source are matched in target users by current display name. When this is set to `true`,
        /// users are matched also by mapped name from user mapping file.
        /// </summary>
        public bool UseUserMapping { get; set; }

        /// <summary>
        /// Mapping of source team names to target team names. Use this when the target project has
        /// differently named teams (e.g. default teams named after the project). If a source team name
        /// is found in this dictionary, the processor will look for the mapped target team name instead
        /// of looking for an exact name match. When a mapping is used and the target team exists,
        /// its settings will be updated regardless of the UpdateTeamSettings flag.
        /// </summary>
        /// <default>{}</default>
        public Dictionary<string, string> TeamMappings { get; set; } = new Dictionary<string, string>();
    }
}
