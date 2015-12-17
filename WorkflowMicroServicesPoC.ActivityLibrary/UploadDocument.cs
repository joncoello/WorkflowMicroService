using MYOB.DAL;
using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WKUK.CCH.Document.DocMgmt.DocManager;

namespace WorkflowMicroServicesPoC.ActivityLibrary
{
    public sealed class UploadDocument : CodeActivity
    {

        public InArgument<string> DocumentPath { get; set; }
        public InArgument<int> ContactID { get; set; }
        public InArgument<int> AssignmentID { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            Console.WriteLine("uploading document...");

            try
            {
                
                string path = DocumentPath.Get(context);
                int contactID = ContactID.Get(context);
                int assignmentID = AssignmentID.Get(context);

                string tempPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(path));
                File.Copy(path, tempPath, true);

                var centralDAL = new DAL("0");
                var docManager = new DocManager(centralDAL);

                var document = new WKUK.CCH.Document.DocMgmt.Entities.Document()
                {
                    LocalPath = tempPath,
                    ImportedDocumentPath = tempPath,
                    Description = Path.GetFileName(tempPath),
                    DocumentTypeId = 6, // permanenent
                    LibraryId = 3, // client
                    CreatedDate = DateTime.Now,
                    CreatedByContactId = 1,
                    SourceId = 3,
                    Name = Path.GetFileName(tempPath),
                    SupressThumbnail = true,
                };
                document.ContactID = contactID;
                document.AssignmentId = assignmentID;
                var documents = new WKUK.CCH.Document.DocMgmt.Entities.DocumentCollection();
                documents.Add(document);
                docManager.UploadAddedDocuments(documents, false, 0);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                throw;
            }
        }
    }
}
