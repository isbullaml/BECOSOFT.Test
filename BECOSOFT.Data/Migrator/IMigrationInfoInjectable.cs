namespace BECOSOFT.Data.Migrator {
    internal interface IMigrationInfoInjectable {
        void Inject(IMigrationInfo migrationInfo);
    }
}