# Azure-Serverless
UserActionFunctionApp conatins 2 Time Trigger Functions and a ServiceBus Trigger Function.

Delete Function is to clean up the DB tables with some obsolete values based on some input parameters. This job is configured  to trigger daily with CRON time format.

Syncronize Function is to to perform the consistency between two remote database tables based on some input parameters. This job is configured to trigger on daily basis which ends up making the sync among DBs.

Create Function is a ServiceBus Trigger function and is depends on the Syncronize User Function. When Sync job starts for each sync the record will be push to Service Bus and that generate the ServiceBusTrigger event and Create job start executing.
