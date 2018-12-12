# Azure-Serverless

- EventReportingService have 3 HttpTrigger functions. The main functionality of this service is to log the events and History of events which is based on Subscription(s), subscribed to an EventGrid.

StoreEventHistroy is to process & filter the the event data based on the subscribed services and store the event details in a cloud table.

StoreStorageevents triggers and logs the event data in storage table based on CRUD operations happens to some of the cloud tables.

- UserActionFunctionApp conatins 2 Time Trigger Functions and a ServiceBus Trigger Function.

Delete Function is to clean up the records from database tables with some obsolete values based on some input parameters. This job is configured  to trigger daily at scheduled time format (CRON).

Syncronize Function is to bring the consistency between two database (storage accounts) tables based on some input parameters. This job is configured to trigger on daily basis which ends up making the sync among DBs.

Create Function is a ServiceBus Trigger function and is depends on the Syncronize User Function (discussed above). When Sync job starts, for each sync operation the record will be push to Service Bus and that generate the ServiceBusTrigger event which end up creating the records doesn't exist in one the DB tables.
