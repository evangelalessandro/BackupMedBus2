using System;
using System.Collections.Generic;

namespace BackupMedBus.Report.PatientDb
{
    public class Patient
    {
        public Patient()
        {
            Episodes = new List<Episode>();
        }

        public Guid Uid
        { get; set; }

        public string Name { get; set; }

        public string SurName { get; set; }

        public string SecondName { get; set; }

        public GenderPatient Gender { get; set; }

        public DateTime BirthDay { get; set; }

        public string Country { get; set; }

        public List<Episode> Episodes
        { get; set; }
    }
}