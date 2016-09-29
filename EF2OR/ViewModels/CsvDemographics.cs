namespace EF2OR.ViewModels
{
    public class CsvDemographics
    {
        [OR10IncludeField]
        public string userSourcedId { get; set; }

        [OR11IncludeField]
        public string sourcedId { get; set; }

        [OR10IncludeField]
        [OR11IncludeField]
        public string status { get; set; }

        [OR10IncludeField]
        [OR11IncludeField]
        public string dateLastModified { get; set; }

        [OR10IncludeField]
        [OR11IncludeField]
        public string birthdate { get; set; }

        [OR10IncludeField]
        [OR11IncludeField]
        public string sex { get; set; }

        [OR10IncludeField]
        [OR11IncludeField]
        public string americanIndianOrAlaskaNative { get; set; }

        [OR10IncludeField]
        [OR11IncludeField]
        public string asian { get; set; }

        [OR10IncludeField]
        [OR11IncludeField]
        public string blackOrAfricanAmerican { get; set; }

        [OR10IncludeField]
        [OR11IncludeField]
        public string nativeHawaiianOrOtherPacificIslander { get; set; }

        [OR10IncludeField]
        [OR11IncludeField]
        public string white { get; set; }

        [OR10IncludeField]
        [OR11IncludeField]
        public string demographicRaceTwoOrMoreRaces { get; set; }

        [OR10IncludeField]
        [OR11IncludeField]
        public string hispanicOrLatinoEthnicity { get; set; }

        [OR10IncludeField]
        [OR11IncludeField]
        public string countryOfBirthCode { get; set; }

        [OR10IncludeField]
        [OR11IncludeField]
        public string stateOfBirthAbbreviation { get; set; }

        [OR10IncludeField]
        [OR11IncludeField]
        public string cityOfBirth { get; set; }

        [OR10IncludeField]
        [OR11IncludeField]
        public string publicSchoolResidenceStatus { get; set; }
    }
}