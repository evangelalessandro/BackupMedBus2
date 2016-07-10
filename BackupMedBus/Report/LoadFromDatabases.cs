using BackupMedBus.Report.PatientDb;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace BackupMedBus.Report
{
    public class LoadFromDatabases : IDisposable
    {
        #region Public Events

        public event EventHandler<LogEventArgs> Log;

        #endregion Public Events

        #region Public Methods

        public void Dispose()
        {
        }

        public void ReadTables(string connectionString, string prefixTable)
        {
            var patientList = new List<PatientDb.Patient>();

            string selectSQL = "SELECT * FROM " + prefixTable + "fmf_patients.PATIENT_IDENTITY WHERE IDENT_ISACTIVE=1 ";
            using (var dataReader
                = getDataReader(connectionString, selectSQL))
            {
                ReadPatientData(dataReader, patientList);
            }

            selectSQL = "SELECT * FROM " + prefixTable + "fmf_episodes.EPISODES";
            using (var dataReader = getDataReader(connectionString, selectSQL))
            {
                ReadEpisodesData(dataReader, patientList);
            }

            var etaEpisodi = patientList.Select(a =>
             new
             {
                 Eta = DateTime.Now.Subtract(a.BirthDay).TotalDays / 364,
                 Episodi = a.Episodes
             }).ToList();

            var dateEpisodi = etaEpisodi
                .SelectMany(a => a.Episodi)
                .Select(a => a.Data.Date)
                .Distinct()
                .OrderByDescending(a => a).ToList();

            foreach (var dataEpisodio in dateEpisodi)
            {
                Log(this,
                    new LogEventArgs
                            ("Day: " + dataEpisodio.ToShortDateString(), LogWarning.Ok));

                var etaConEpisodiQuelGiornoSpecifico = etaEpisodi.Where(a => a.Episodi.Where(b =>
                  b.Data == dataEpisodio).Count() > 0).Select(a => new { Eta = a.Eta }).ToList();
                var eta0_4 = etaConEpisodiQuelGiornoSpecifico.Where(a => a.Eta <= 4).Count();

                Log(this,
                    new LogEventArgs
                            ("Patient 0-4 year: " + eta0_4.ToString(),
                            LogWarning.Ok));

                var eta5_17 = etaConEpisodiQuelGiornoSpecifico.Where(a => a.Eta <= 17 && a.Eta >= 5).Count();

                Log(this,
                    new LogEventArgs
                            ("Patient 5-17 year: " + eta5_17.ToString(),
                            LogWarning.Ok));

                var eta18_other = etaConEpisodiQuelGiornoSpecifico.Where(a => a.Eta >= 18).Count();

                Log(this,
                    new LogEventArgs
                            ("Patient 18+ year: " + eta18_other.ToString(),
                            LogWarning.Ok));
                Log(this,
                    new LogEventArgs
                            (Environment.NewLine, LogWarning.Ok));
            }

            Log(this,
                    new LogEventArgs
                            ("Report OK "
                            , LogWarning.Ok));
        }

        #endregion Public Methods

        #region Private Methods

        private MySqlDataReader getDataReader(string connectionString, string selectSQL)
        {
            var conread =
                new MySqlConnection(connectionString);
            {
                conread.Open();
                using (var selectCommand = new MySqlCommand(selectSQL, conread))
                {
                    return selectCommand.ExecuteReader();
                }
            }
        }

        private void ReadEpisodesData(MySqlDataReader dataReader, List<PatientDb.Patient> patientList)
        {
            while (dataReader.Read())
            {
                if (dataReader["PATIENT_UID"] != DBNull.Value)
                {
                    var itemPatient = patientList.Where(a => a.Uid == new Guid(dataReader["PATIENT_UID"].ToString())).FirstOrDefault();

                    if (itemPatient != null)
                    {
                        itemPatient.Episodes.Add(
                            new Episode()
                            { Data = ((DateTime)dataReader["DATECREATION"]).Date });
                    }
                }
            }
        }

        private void ReadPatientData(MySqlDataReader dataReader, List<PatientDb.Patient> patientList)
        {
            while (dataReader.Read())
            {
                var newPatient = new PatientDb.Patient();
                newPatient.Country = dataReader["Country"].ToString();
                newPatient.BirthDay = (DateTime)dataReader["DOB"];
                if (dataReader["GENDER"].ToString() == "M")
                {
                    newPatient.Gender = GenderPatient.Male;
                }
                else if (dataReader["GENDER"].ToString() == "F")
                {
                    newPatient.Gender = GenderPatient.Male;
                }
                else
                {
                    newPatient.Gender = GenderPatient.Unknow;
                }
                newPatient.Name = dataReader["NAME"].ToString();
                newPatient.SecondName = dataReader["SECONDNAME"].ToString();
                newPatient.SurName = dataReader["SURNAME"].ToString();
                newPatient.Uid = new Guid(dataReader["IDENT_UID"].ToString());

                patientList.Add(newPatient);
            }
        }

        #endregion Private Methods
    }
}