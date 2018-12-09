

Provision Azure Environment
===========================

1) Login to the Azure portal account.

$ Login-AzureRmAccount

2) Create a resource group for the Event Report Service.

$ New-AzureRmResourceGroup -Name "EventReportingService-${ENVIRONMENT_NAME}" -Location "West US"

3) Provision the Event Report Service environment. The available environments are Dev, DevTest, Test, Stage and Prod.

$ New-AzureRmResourceGroupDeployment -ResourceGroupName "EventReportingService-${ENVIRONMENT_NAME}" -TemplateFile "ServiceTemplate.json" -TemplateParameterFile "ServiceParameters-${ENVIRONMENT_NAME}.json"


Deploy instructions
==================

1) Right-click on the EventReportingService and select Publish...

2) From the target "Azure Function App" choose "Select Existing"

3) Select the function that was provisioned above and then complete the publishing


Provision Event Subscriptions
=============================
This must be performed after the code has been deployed as it needs to know the key of the deployed Azure Function.

1) Provision the event subscriptions required by the Event Report Service. The available environments are Dev, DevTest, Test, Stage and Prod.

$ New-AzureRmResourceGroupDeployment -ResourceGroupName "-${ENVIRONMENT_NAME}" -TemplateFile "EventsTemplate.json" -TemplateParameterFile "EventsParameters-${ENVIRONMENT_NAME}.json"

