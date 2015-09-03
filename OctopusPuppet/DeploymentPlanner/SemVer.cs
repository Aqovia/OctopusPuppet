using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OctopusPuppet.DeploymentPlanner
{
    /// <summary>
    /// A hybrid implementation of SemVer that supports semantic versioning as described at http://semver.org while not
    ///             strictly enforcing it to
    ///             allow older 4-digit versioning schemes to continue working.
    /// 
    /// </summary>
    [Serializable]
    public sealed class SemVer : IComparable, IComparable<SemVer>, IEquatable<SemVer>
    {
        private static readonly Regex _semanticVersionRegex = new Regex("^(?<Version>\\d+(\\s*\\.\\s*\\d+){0,3})(?<Release>-[a-z][0-9a-z-]*)?$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        private static readonly Regex _strictSemanticVersionRegex = new Regex("^(?<Version>\\d+(\\.\\d+){2})(?<Release>-[a-z][0-9a-z-]*)?$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        private const RegexOptions _flags = RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled;
        private readonly string _originalString;

        /// <summary>
        /// Gets the normalized version portion.
        /// 
        /// </summary>
        public Version Version { get; private set; }

        /// <summary>
        /// Gets the optional special version.
        /// 
        /// </summary>
        public string SpecialVersion { get; private set; }

        public SemVer(string version)
            : this(SemVer.Parse(version, false))
        {
            this._originalString = version;
        }

        public SemVer(int major, int minor, int build, int revision)
            : this(new Version(major, minor, build, revision))
        {
        }

        public SemVer(int major, int minor, int build, string specialVersion)
            : this(new Version(major, minor, build), specialVersion)
        {
        }

        public SemVer(Version version)
            : this(version, string.Empty)
        {
        }

        public SemVer(Version version, string specialVersion)
            : this(version, specialVersion, (string)null, false)
        {
        }

        private SemVer(Version version, string specialVersion, string originalString, bool preserveMissincComponents = false)
        {
            if (version == (Version)null)
                throw new ArgumentNullException("version");
            this.Version = preserveMissincComponents ? version : SemVer.NormalizeVersionValue(version);
            this.SpecialVersion = specialVersion ?? string.Empty;
            this._originalString = string.IsNullOrEmpty(originalString) ? (string)(object)version + (!string.IsNullOrEmpty(specialVersion) ? (object)("-" + specialVersion) : (object)(string)null) : originalString;
        }

        internal SemVer(SemVer semVer)
        {
            this._originalString = semVer.ToString();
            this.Version = semVer.Version;
            this.SpecialVersion = semVer.SpecialVersion;
        }

        public static bool operator ==(SemVer version1, SemVer version2)
        {
            if (version1 == null)
                return version2 == null;
            return version1.Equals(version2);
        }

        public static bool operator !=(SemVer version1, SemVer version2)
        {
            return !(version1 == version2);
        }

        public static bool operator <(SemVer version1, SemVer version2)
        {
            if (version1 == (SemVer)null)
                throw new ArgumentNullException("version1");
            return version1.CompareTo(version2) < 0;
        }

        public static bool operator <=(SemVer version1, SemVer version2)
        {
            if (!(version1 == version2))
                return version1 < version2;
            return true;
        }

        public static bool operator >(SemVer version1, SemVer version2)
        {
            if (version1 == (SemVer)null)
                throw new ArgumentNullException("version1");
            return version2 < version1;
        }

        public static bool operator >=(SemVer version1, SemVer version2)
        {
            if (!(version1 == version2))
                return version1 > version2;
            return true;
        }

        /// <summary>
        /// Parses a version string using loose semantic versioning rules that allows 2-4 version components followed by an
        ///             optional special version.
        /// 
        /// </summary>
        public static SemVer Parse(string version, bool preserveMissingComponents = false)
        {
            if (string.IsNullOrEmpty(version))
                throw new ArgumentException("Argument cannot be null or empty", "version");
            SemVer semanticVersion;
            if (!SemVer.TryParse(version, out semanticVersion, preserveMissingComponents))
            {
                CultureInfo currentCulture = CultureInfo.CurrentCulture;
                string format = "Invalid version string {0}";
                object[] objArray = new object[1];
                int index = 0;
                string str = version;
                objArray[index] = (object)str;
                throw new ArgumentException(string.Format((IFormatProvider)currentCulture, format, objArray), "version");
            }
            return semanticVersion;
        }

        /// <summary>
        /// Parses a version string using loose semantic versioning rules that allows 2-4 version components followed by an
        ///             optional special version.
        /// 
        /// </summary>
        public static bool TryParse(string version, out SemVer value, bool preserveMissingComponents = false)
        {
            return SemVer.TryParseInternal(version, SemVer._semanticVersionRegex, out value, preserveMissingComponents);
        }

        /// <summary>
        /// Parses a version string using strict semantic versioning rules that allows exactly 3 components and an optional
        ///             special version.
        /// 
        /// </summary>
        public static bool TryParseStrict(string version, out SemVer value, bool preserveMissingComponents = false)
        {
            return SemVer.TryParseInternal(version, SemVer._strictSemanticVersionRegex, out value, preserveMissingComponents);
        }

        private static bool TryParseInternal(string version, Regex regex, out SemVer semVer,
            bool preserveMissingComponents = false)
        {
            semVer = (SemVer) null;
            if (string.IsNullOrEmpty(version))
                return false;
            Match match = regex.Match(version.Trim());
            Version result;
            if (!match.Success || !Version.TryParse(match.Groups["Version"].Value, out result))
                return false;
            Version version1 = preserveMissingComponents ? result : SemVer.NormalizeVersionValue(result);
            Version version2 = version1;
            string str = match.Groups["Release"].Value;
            char[] chArray = new char[1];
            int index = 0;
            int num1 = 45;
            chArray[index] = (char) num1;
            string specialVersion = str.TrimStart(chArray);
            string originalString = version.Replace(" ", "");
            int num2 = preserveMissingComponents ? 1 : 0;
            SemVer semanticVersion = new SemVer(version2, specialVersion, originalString, num2 != 0);

            return true;
        }

        /// <summary>
        /// Attempts to parse the version token as a SemanticVersion.
        /// 
        /// </summary>
        /// 
        /// <returns>
        /// An instance of SemanticVersion if it parses correctly, null otherwise.
        /// </returns>
        public static SemVer ParseOptionalVersion(string version)
        {
            SemVer semanticVersion;
            SemVer.TryParse(version, out semanticVersion, false);
            return semanticVersion;
        }

        private static Version NormalizeVersionValue(Version version)
        {
            return new Version(version.Major, version.Minor, Math.Max(version.Build, 0), Math.Max(version.Revision, 0));
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            SemVer other = obj as SemVer;
            if (other == (SemVer)null)
                throw new ArgumentException("Type must be a semantic version", "obj");
            return this.CompareTo(other);
        }

        public int CompareTo(SemVer other)
        {
            if (other == null)
                return 1;
            int num = this.Version.CompareTo(other.Version);
            if (num != 0)
                return num;
            bool flag1 = string.IsNullOrEmpty(this.SpecialVersion);
            bool flag2 = string.IsNullOrEmpty(other.SpecialVersion);
            if (flag1 & flag2)
                return 0;
            if (flag1)
                return 1;
            if (flag2)
                return -1;
            return StringComparer.OrdinalIgnoreCase.Compare(this.SpecialVersion, other.SpecialVersion);
        }

        public override string ToString()
        {
            return this._originalString;
        }

        public bool Equals(SemVer other)
        {
            if (other == null)
                return false;
            if (this == other)
                return true;
            if (object.Equals((object)other.Version, (object)this.Version))
                return object.Equals((object)other.SpecialVersion, (object)this.SpecialVersion);
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this == obj)
                return true;
            if (obj.GetType() != typeof(SemVer))
                return false;
            return this.Equals((SemVer)obj);
        }

        public override int GetHashCode()
        {
            return (this.Version != (Version)null ? this.Version.GetHashCode() : 0) * 397 ^ (this.SpecialVersion != null ? this.SpecialVersion.GetHashCode() : 0);
        }

        public SemVer Increment()
        {
            return new SemVer(this.Version.Revision == -1 ? (this.Version.Build == -1 ? new Version(this.Version.Major, this.Version.Minor + 1) : new Version(this.Version.Major, this.Version.Minor, this.Version.Build + 1)) : new Version(this.Version.Major, this.Version.Minor, this.Version.Build, this.Version.Revision + 1), this.SpecialVersion);
        }

        public static string IncrementSpecial(string specialVersion)
        {
            if (specialVersion == null)
                throw new ArgumentNullException("specialVersion");
            char[] chArray = Enumerable.ToArray<char>(Enumerable.Reverse<char>((IEnumerable<char>)specialVersion));
            Func<char, bool> predicate1 = new Func<char, bool>(char.IsDigit);
            string str1 = new string(Enumerable.ToArray<char>(Enumerable.Reverse<char>(Enumerable.TakeWhile<char>((IEnumerable<char>)chArray, predicate1))));
            Func<char, bool> predicate2 = new Func<char, bool>(char.IsDigit);
            string str2 = new string(Enumerable.ToArray<char>(Enumerable.Reverse<char>(Enumerable.SkipWhile<char>((IEnumerable<char>)chArray, predicate2))));
            return !(str1 != "") ? str2 + "2" : str2 + (object)(BigInteger.Parse(str1) + (BigInteger)1);
        }
    }
}
