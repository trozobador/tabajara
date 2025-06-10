using Microsoft.Xrm.Sdk;
using System;
using System.Text.RegularExpressions;

public class FormatPhoneNumberPlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
        var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
        var service = serviceFactory.CreateOrganizationService(context.UserId);

        // Verifica se está no PreOperation e nas mensagens desejadas
        if (context.Stage != 20 || (context.MessageName != "Create" && context.MessageName != "Update"))
            return;

        if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity entity))
            return;

        if (entity.LogicalName != "contact")
            return;

        if (!entity.Attributes.Contains("telephone1") || entity["telephone1"] == null)
            return;

        var telefoneOriginal = entity["telephone1"].ToString();
        string telefoneNumerico = Regex.Replace(telefoneOriginal, "[^0-9]", "");

        // Limita aos últimos 11 dígitos se houver mais
        if (telefoneNumerico.Length > 11)
            telefoneNumerico = telefoneNumerico.Substring(telefoneNumerico.Length - 11);

        if (telefoneNumerico.Length < 10)
            throw new InvalidPluginExecutionException("O número de telefone é muito curto. Insira pelo menos 10 dígitos.");

        string telefoneFormatado;

        if (telefoneNumerico.Length == 11)
        {
            telefoneFormatado = $"({telefoneNumerico.Substring(0, 2)}) {telefoneNumerico.Substring(2, 5)}-{telefoneNumerico.Substring(7, 4)}";
        }
        else // 10 dígitos
        {
            telefoneFormatado = $"({telefoneNumerico.Substring(0, 2)}) {telefoneNumerico.Substring(2, 4)}-{telefoneNumerico.Substring(6, 4)}";
        }

        entity["telephone1"] = telefoneFormatado;
    }
}
