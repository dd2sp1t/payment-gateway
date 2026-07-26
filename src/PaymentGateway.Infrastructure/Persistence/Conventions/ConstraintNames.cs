using PaymentGateway.Infrastructure.Persistence.Constants;
using PaymentGateway.Infrastructure.Persistence.Entities;

namespace PaymentGateway.Infrastructure.Persistence.Conventions;

internal static class ConstraintNames
{
    public static string OperationsPrimaryKey =>
        PrimaryKey(table: DatabaseTables.Operations);

    public static string OperationsProviderPaymentIdUnique =>
        Unique(
            table: DatabaseTables.Operations,
            column: nameof(DbOperation.ProviderPaymentId));

    public static string OperationEventsPrimaryKey =>
        PrimaryKey(table: DatabaseTables.OperationEvents);

    public static string OperationEventsOperationForeignKey =>
        ForeignKey(
            dependentTable: DatabaseTables.OperationEvents,
            principalTable: DatabaseTables.Operations,
            column: nameof(DbOperationEvent.OperationId));

    private static string PrimaryKey(string table) =>
        $"pk_{table.ToLowerInvariant()}";

    private static string Unique(string table, string column) =>
        $"ux_{table.ToLowerInvariant()}_{column.ToLowerInvariant()}";

    private static string ForeignKey(string dependentTable, string principalTable, string column) =>
        $"fk_{dependentTable.ToLowerInvariant()}_{principalTable.ToLowerInvariant()}_{column.ToLowerInvariant()}";
}