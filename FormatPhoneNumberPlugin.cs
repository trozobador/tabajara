using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

public class PreCreateRegistroVisitaPlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
        var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
        var service = serviceFactory.CreateOrganizationService(context.UserId);
        var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

        if (context.MessageName != "Create" || context.Stage != 20)
            return;

        if (!(context.InputParameters["Target"] is Entity entity) || entity.LogicalName != "vc4_registrodevisita")
            return;

        string valores = "";
        int tipoRegistro = entity.GetAttributeValue<OptionSetValue>("vc4_tipoderegistro")?.Value ?? -1;

        string fieldToUse = tipoRegistro == 947750000 ? "vc4_objetivodavisita" : "vc4_demandarecebida";

        if (!entity.Attributes.Contains(fieldToUse))
            return;

        var optionSetValues = entity.GetAttributeValue<OptionSetValueCollection>(fieldToUse);

        if (optionSetValues == null || !optionSetValues.Any())
            return;

        var labels = GetOptionSetLabels(service, "vc4_registrodevisita", fieldToUse, optionSetValues);
        valores = string.Join(", ", labels);

        entity["vc4_name"] = valores;
    }

    private List<string> GetOptionSetLabels(IOrganizationService service, string entityName, string fieldName, OptionSetValueCollection values)
    {
        var labels = new List<string>();

        var request = new RetrieveAttributeRequest
        {
            EntityLogicalName = entityName,
            LogicalName = fieldName,
            RetrieveAsIfPublished = true
        };

        var response = (RetrieveAttributeResponse)service.Execute(request);
        var metadata = (MultiSelectPicklistAttributeMetadata)response.AttributeMetadata;

        foreach (var value in values)
        {
            var option = metadata.OptionSet.Options.FirstOrDefault(o => o.Value == value.Value);
            if (option != null)
                labels.Add(option.Label.UserLocalizedLabel.Label);
        }

        return labels;
    }
}
