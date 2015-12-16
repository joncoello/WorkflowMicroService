using MYOB.CSSTaskManagement;
using MYOB.DAL;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkflowMicroServicesPoC.ActivityLibrary
{
    public sealed class CreateTask : CodeActivity
    {

        public InArgument<int> ContactID { get; set; }
        public InArgument<int> EmployeeID { get; set; }

        protected override void Execute(CodeActivityContext context)
        {

            Console.WriteLine("Creating task...");

            try
            {

                int contactID = ContactID.Get(context);
                int employeeID = EmployeeID.Get(context);

                var dal = new DAL("0");
                var task = new CSSTask(dal);
                task.Description = "Money Laundering checks for client";
                task.CodeId = 8;
                task.Save();

                task.AssignToContactAssignment(CSSTask.CSSAssignToType.Contact, contactID);
                task.AssignTo(employeeID, 7, DateTime.Now, "Get on with it", 1);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                throw;
            }

        }

    }

}
