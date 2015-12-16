using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using MYOB.DAL;
using Central.CSSContactAPI;

namespace WorkflowMicroServicesPoC.ActivityLibrary
{

    public sealed class CreateClient : CodeActivity
    {
        public InArgument<string> ClientCode { get; set; }
        public InArgument<string> Name { get; set; }

        public OutArgument<int> ContactID { get; set; }
        public OutArgument<int> ClientID { get; set; }

        protected override void Execute(CodeActivityContext context)
        {

            Console.WriteLine("Creating client...");

            try
            {
                string clientCode = context.GetValue(this.ClientCode);
                string name = context.GetValue(this.Name);

                var dal = new DAL("0");
                var gateway = new CentralGateway(dal);
                var contact = new Organisation()
                {
                    Name = name
                };
                gateway.Save(contact);
                gateway.ConvertContactToClient(contact, clientCode, 7);

                context.SetValue(this.ContactID, contact.ContactId);
                context.SetValue(this.ClientID, contact.Client.ClientId);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                throw;
            }



        }
    }
}
