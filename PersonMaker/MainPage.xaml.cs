using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Microsoft.ProjectOxford.Face;
//using Microsoft.ProjectOxford.Face_edit;
using Microsoft.ProjectOxford.Face.Contract;
using Windows.System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI;
using System.Runtime.InteropServices;





// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PersonMaker
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        string authKey;
        string personGroupId;
        string personGroupName;
        

        Guid personId;
        string personName;
        string person_instagramId;
        string person_twitterId;
        string social_handle;
        StorageFolder personFolder;
        // PersonInstagramhandleTextBox
        private FaceServiceClient faceServiceClient;
        //private faceServiceCLIENT faceServiceClient;
        private PersonGroup knownGroup;
        private int minPhotos = 6;

        public MainPage()
        {
            this.InitializeComponent();
            personName = string.Empty;
            authKey = string.Empty;
            personGroupId = string.Empty;
            personGroupName = string.Empty;
            personId = Guid.Empty;
            person_twitterId = string.Empty;
            person_instagramId = string.Empty;
            social_handle = string.Empty;



        }

        /// <summary>
        /// Create a person group with ID and name provided if none can be found in the service.
        /// </summary>
        private async void CreatePersonGroupButton_ClickAsync(object sender, RoutedEventArgs e)
        {

            personGroupId = "maketwitter"; //"1122";
            personGroupName = "1122"; //"maketwitter";
            PersonGroupStatusTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            authKey = "69e550a6bafc449b8f90bb2c56e5d846";//"abbd309cfe8a4f8393713f14fefbfe42"; //"abbd309cfe8a4f8393713f14fefbfe42";//"69e550a6bafc449b8f90bb2c56e5d846";//AuthKeyTextBox.Text; ///Enter the authorization code for Azure Here.

            if (string.IsNullOrWhiteSpace(personGroupId) == false  && string.IsNullOrWhiteSpace(authKey) == false) //&& string.IsNullOrWhiteSpace(personGroupName) == false
            {
                PersonGroupCreateErrorText.Visibility = Visibility.Collapsed;
                await ApiCallAllowed(true);
                faceServiceClient = new FaceServiceClient(authKey);

                if (null != faceServiceClient)
                {
                    // You may experience issues with this below call, if you are attempting connection with
                    // a service location other than 'West US'
                    PersonGroup[] groups = await faceServiceClient.ListPersonGroupsAsync();
                    var matchedGroups = groups.Where(p => p.PersonGroupId == personGroupId);

                    if (matchedGroups.Count() > 0)
                    {
                        knownGroup = matchedGroups.FirstOrDefault();

                        PersonGroupStatusTextBlock.Text = "Joined Group: " + knownGroup.Name;
                    }

                    if (null == knownGroup)
                    {
                        await ApiCallAllowed(true);
                        await faceServiceClient.CreatePersonGroupAsync(personGroupId, personGroupName);
                        knownGroup = await faceServiceClient.GetPersonGroupAsync(personGroupId);

                        PersonGroupStatusTextBlock.Text = "Created new group: " + knownGroup.Name;
                    }

                    if (PersonGroupStatusTextBlock.Text != "- Person Group status -")
                    {
                        PersonGroupStatusTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        PersonGroupStatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    }
                }
            }
            else
            {
                PersonGroupCreateErrorText.Text = "Make sure that you have entered the proper group name";
                PersonGroupCreateErrorText.Visibility = Visibility.Visible;
            }
        }

        private async void FetchPersonGroup_Click(object sender, RoutedEventArgs e)
        {
            personGroupId = "maketwitter"; //"1122";
            personGroupName = "1122"; //"maketwitter";
            PersonGroupStatusTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            authKey = "69e550a6bafc449b8f90bb2c56e5d846"; //"abbd309cfe8a4f8393713f14fefbfe42"; ////AuthKeyTextBox.Text;

            await ApiCallAllowed(true);
            faceServiceClient = new FaceServiceClient(authKey);

            if (null != faceServiceClient)
            {
                // You may experience issues with this below call, if you are attempting connection with
                // a service location other than 'West US'
                PersonGroup[] groups = await faceServiceClient.ListPersonGroupsAsync();
                var matchedGroups = groups.Where(p => p.PersonGroupId == personGroupId);

                if (matchedGroups.Count() > 0)
                {
                    knownGroup = matchedGroups.FirstOrDefault();

                    PersonGroupStatusTextBlock.Text = "Joined Group: " + knownGroup.Name; //"Please enter the following details below";// + knownGroup.Name;//"Successfully Joined Social Lens";//
                }

                if (null == knownGroup)
                {
                    PersonGroupStatusTextBlock.Text = "Could not find group. Make sure that you have entered the proper group name ";// + knownGroup.Name;
                }
                PersonGroupStatusTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                /*
                if (PersonGroupStatusTextBlock.Text.ToLower().Contains("please"))
                {
                    PersonGroupStatusTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    PersonGroupStatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                }
                */
            }
        }

        private async void CreatePersonButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            personName = PersonNameTextBox.Text;
            person_twitterId = PersonTwitterhandleTextBox.Text;
            person_instagramId = PersonInstagramhandleTextBox.Text;
            social_handle = String.Concat(person_twitterId, '|'); // '|' is used as the unique char to split the
            social_handle = String.Concat(social_handle, person_instagramId);
            PersonStatusTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            if (knownGroup != null && personName.Length > 0 && person_twitterId.Length > 0 && person_instagramId.Length > 0)
            {
                CreatePersonErrorText.Visibility = Visibility.Collapsed;
                //Check if this person already exist
                bool personAlreadyExist = false;
                Person[] ppl = await GetKnownPeople();
                foreach (Person p in ppl)
                {
                    if (p.Name == personName)
                    {
                        personAlreadyExist = true;
                        PersonStatusTextBlock.Text = $"Person already exist: {p.Name} ID: {p.PersonId}";

                        PersonStatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    }
                }

                if (!personAlreadyExist)
                {
                    await ApiCallAllowed(true);
                    CreatePersonResult result = await faceServiceClient.CreatePersonAsync(personGroupId, personName, social_handle);

                    if (null != result && null != result.PersonId)
                    {
                        personId = result.PersonId;

                        //PersonStatusTextBlock.Text = "Created new account" + result.PersonId;

                        ///
                        /*
                        try
                        {
                            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                            builder.DataSource = "sociallens-cs538.database.windows.net";
                            builder.UserID = "sociallens";
                            builder.Password = "Password!12";
                            builder.InitialCatalog = "SocialLens";
                            SqlConnection connection1 = new SqlConnection(builder.ConnectionString);
                            
                            PersonStatusTextBlock.Text = "hello "+connection1.State.ToString();
                            
                            //using ()
                            {
                                //Console.WriteLine("\nQuery data example:");
                                //Console.WriteLine("=========================================\n");
                                connection1.Open();
                                //connection.Open();
                                StringBuilder sb = new StringBuilder();
                                sb.Append("insert into persons values('"+result.PersonId+ "','"+ personName+"','" +person_facebookId+ "','"+person_facebookId+"','"+person_facebookId+ "','" + person_facebookId + "'); ");
                                String sql = sb.ToString();
                                
                                using (SqlCommand command = new SqlCommand(sql, connection1))
                                {
                                    using (SqlDataReader reader = command.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            //Console.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(1));
                                        }
                                    }
                                }
                                
                            }
                        
                        }
                        
                        catch (SqlException ee)
                        {
                            PersonStatusTextBlock.Text = ee.ToString();//"Created new account" + result.PersonId;//Console.WriteLine(e.ToString());
                        }
                        //Console.ReadLine()

                        */
                        ///
                        PersonStatusTextBlock.Text = "Created new account" + result.PersonId;

                        PersonStatusTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                    }
                }
            }
            else
            {
                CreatePersonErrorText.Text = "Please provide Name and Twitter handle above.";
                CreatePersonErrorText.Visibility = Visibility.Visible;
            }
        }

     

        private async void CreateFolderButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (personName.Length > 0 && personId != Guid.Empty)
            {
                CreateFolderErrorText.Visibility = Visibility.Collapsed;
                StorageFolder picturesFolder = KnownFolders.PicturesLibrary;
                personFolder = await picturesFolder.CreateFolderAsync(personName, CreationCollisionOption.OpenIfExists);
                await Launcher.LaunchFolderAsync(personFolder);
            }
            else
            {
                CreateFolderErrorText.Text = "You must have created a person in section 3.";
                CreateFolderErrorText.Visibility = Visibility.Visible;
            }
        }



        private async void SubmitToAzureButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            string successfullySubmitted = string.Empty;
            SubmissionStatusTextBlock.Foreground = new SolidColorBrush(Colors.Green);

            int imageCounter = 0;
            if (null != personFolder)
            {
                var items = await personFolder.GetFilesAsync();

                if (items.Count > 0)
                {
                    List<StorageFile> imageFilesToUpload = new List<StorageFile>();
                    foreach (StorageFile item in items)
                    {
                        //Windows Cam default save type is jpg
                        if (item.FileType.ToLower() == ".jpg" || item.FileType.ToLower() == ".png")
                        {
                            imageCounter++;
                            imageFilesToUpload.Add(item);
                        }
                        else
                        {
                            Debug.WriteLine(string.Format("Photo {0}, from {1}, is in the wrong format. Images must be jpg or png!", item.DisplayName, item.Path));
                        }
                    }

                    if (imageCounter >= minPhotos)
                    {
                        imageCounter = 0;
                        try
                        {
                            foreach (StorageFile imageFile in imageFilesToUpload)
                            {
                                imageCounter++;
                                using (Stream s = await imageFile.OpenStreamForReadAsync())
                                {
                                    await ApiCallAllowed(true);
                                    AddPersistedFaceResult addResult = await faceServiceClient.AddPersonFaceAsync(personGroupId, personId, s);
                                    Debug.WriteLine("Add result: " + addResult + addResult.PersistedFaceId);
                                }
                                SubmissionStatusTextBlock.Text = string.Format("Submission Status: {0}", imageCounter);
                            }
                            SubmissionStatusTextBlock.Text = "Submission Status: Total Images submitted: " + imageCounter;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Submission Exc: " + ex.Message);
                        }
                    }
                    else
                    {
                        SubmissionStatusTextBlock.Text = $"Submission Status: Please add at least {minPhotos} face images to the person folder.";
                    }
                }
                else
                {
                    successfullySubmitted = "Submission Status: No Image Files Found.";
                }
            }
            else
            {
                successfullySubmitted = "Submission Status: No person folder found! Have you completed section five?";
            }

            if (successfullySubmitted != string.Empty)
            {
                SubmissionStatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                SubmissionStatusTextBlock.Text = successfullySubmitted;
            }
            else
            {
                SubmissionStatusTextBlock.Text = "Submission completed successfully! Now train your service!";
            }
        }

        private async void TrainButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (personGroupId.Length > 0)
            {
                TrainStatusTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                await ApiCallAllowed(true);
                await faceServiceClient.TrainPersonGroupAsync(personGroupId);

                TrainingStatus trainingStatus = null;
                while (true)
                {
                    await ApiCallAllowed(true);
                    trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId);

                    if (trainingStatus.Status != Status.Running)
                    {
                        break;
                    }
                    await Task.Delay(1000);
                }

                TrainStatusTextBlock.Text = "Submission Status: Training Completed!";
            }
            else
            {
                TrainStatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                TrainStatusTextBlock.Text = "Submission Status: No person group ID found. Have you completed section two?";
            }
        }

        private async void DeletePersonButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            personName = PersonNameTextBox.Text;

            if (string.IsNullOrWhiteSpace(personName) == false)
            {
                CreatePersonErrorText.Visibility = Visibility.Collapsed;
                bool personExist = false;
                Person[] ppl = await GetKnownPeople();
                foreach (Person p in ppl)
                {
                    if (p.Name == personName)
                    {
                        personExist = true;
                        PersonStatusTextBlock.Text = $"Deleting person: {p.Name} ID: {p.PersonId}";
                        await RemovePerson(p);
                    }
                }
                if (!personExist)
                {
                    PersonStatusTextBlock.Text = $"No persons found to delete.";
                }
            }
            else
            {
                CreatePersonErrorText.Text = "Cannot delete: No name has been provided.";
                CreatePersonErrorText.Visibility = Visibility.Visible;
            }
        }

        internal async Task<Person[]> GetKnownPeople()
        {
            Person[] people = null;
            if (null != faceServiceClient)
            {
                await ApiCallAllowed(true);
                people = await faceServiceClient.ListPersonsAsync(personGroupId);
            }
            return people;
        }

        internal async Task RemovePerson(Person person)
        {
            if (null != person)
            {
                await ApiCallAllowed(true);
                await faceServiceClient.DeletePersonAsync(personGroupId, person.PersonId);
            }
        }

        #region Image Upload Throttling

        public int apiMaxCallsPerMinute = 20;
        [DllImport("kernel32")]
        extern static UInt64 GetTickCount64();
        public List<UInt64> apiCallTimes = new List<UInt64>();
        

        public void NoteApiCallTime()
        {
            apiCallTimes.Add(GetTickCount64());
        }

        public async Task ApiCallAllowed(bool addAnApiCall)
        {
            bool throttleActive = false;
            UInt64 now = GetTickCount64();
            UInt64 boundary = now - 60 * 1000; // one minute ago
            // remove any in list longer than one minute ago
            while (true && apiCallTimes.Count > 0)
            {
                UInt64 sample = apiCallTimes[0];
                if (sample < boundary)
                {
                    apiCallTimes.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }

            if (apiCallTimes.Count >= apiMaxCallsPerMinute)
            {
                throttleActive = true;
                Debug.WriteLine("forced to wait for " + (61 * 1000 - (int)(now - apiCallTimes[0])));
                await Task.Delay(61 * 1000 - (int)(now - apiCallTimes[0]));
            }
            if (addAnApiCall)
            {
                NoteApiCallTime();
            }

            ThrottlingActive.Foreground = new SolidColorBrush(throttleActive == true ? Colors.Red : Colors.Green);
            ThrottlingActive.Text = string.Format("Throttling Status: {0}", throttleActive == true ? "ACTIVE!" : "IN-ACTIVE");
        }


        #endregion

        private void PersonGroupNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void PersonGroupIdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
