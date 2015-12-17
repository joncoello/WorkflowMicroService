using Central.CSSContactAPI;
using MYOB.DAL;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkflowMicroServicesPoC.ActivityLibrary
{
    public sealed class CreateAssignment : CodeActivity
    {

        public InArgument<int> ContactID { get; set; }
        public InArgument<int> AssignmentTypeID { get; set; }
        public InArgument<string> Code{ get; set; }
        public InArgument<string> Name { get; set; }
        public OutArgument<int> AssignmentID { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            Console.WriteLine("Creating assignment...");

            try
            {
                int contactID = context.GetValue(this.ContactID);
                int assignmentTypeID = context.GetValue(this.AssignmentTypeID);
                string code = context.GetValue(this.Code);
                string name = context.GetValue(this.Name);

                var dal = new DAL("0");
                var gateway = new CentralGateway(dal);
                var assignmentType = gateway.FindAssignmentType(assignmentTypeID);
                var contact = gateway.FindContact(contactID, 7);
                var assignment = new Assignment()
                {
                    Type = assignmentType,
                    Client = contact.Client,
                    Code =  code,
                    Name = name
                };
                
                gateway.Save(assignment);

                AssignmentID.Set(context, assignment.AssignmentId);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                throw;
            }
        }

    }

}
