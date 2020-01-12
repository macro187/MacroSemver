using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MacroSemver
{

    /// <summary>
    /// A semantic version
    /// </summary>
    ///
    public sealed class SemVersion : IComparable<SemVersion> , IComparable
    {

        static Regex parseEx = new Regex(
            @"^(?<major>\d+)" +
            @"(\.(?<minor>\d+))?" +
            @"(\.(?<patch>\d+))?" +
            @"(\-(?<pre>[0-9A-Za-z\-\.]+))?" +
            @"(\+(?<build>[0-9A-Za-z\-\.]+))?$",
            RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);


        /// <summary>
        /// Parse a string as a semantic version
        /// </summary>
        ///
        /// <param name="version">
        /// A string containing a semantic version
        /// </param>
        ///
        /// <param name="strict">
        /// If <c>true</c>, minor and patch version are required, else they default to 0
        /// </param>
        ///
        /// <returns>
        /// The <see cref="SemVersion"/> object
        /// </returns>
        ///
        /// <exception cref="System.InvalidOperationException">
        /// An invalid version string is passed
        /// </exception>
        ///
        public static SemVersion Parse(string version, bool strict = false)
        {
            var match = parseEx.Match(version);
            if (!match.Success)
            {
                throw new ArgumentException("Invalid version.", "version");
            }

            var major = int.Parse(match.Groups["major"].Value, CultureInfo.InvariantCulture);

            var minorMatch = match.Groups["minor"];
            int minor = 0;
            if (minorMatch.Success) 
            {
                minor = int.Parse(minorMatch.Value, CultureInfo.InvariantCulture);
            }
            else if (strict)
            {
                throw new InvalidOperationException("Invalid version (no minor version given in strict mode)");
            }

            var patchMatch = match.Groups["patch"];
            int patch = 0;
            if (patchMatch.Success)
            {
                patch = int.Parse(patchMatch.Value, CultureInfo.InvariantCulture);
            }
            else if (strict) 
            {
                throw new InvalidOperationException("Invalid version (no patch version given in strict mode)");
            }

            var prerelease = match.Groups["pre"].Value;
            var build = match.Groups["build"].Value;

            return new SemVersion(major, minor, patch, prerelease, build);
        }


        /// <summary>
        /// Try to parse a string as a semantic version
        /// </summary>
        ///
        /// <param name="version">
        /// The version string
        /// </param>
        ///
        /// <param name="semver">
        /// When the method returns, contains a SemVersion instance equivalent to the version string passed in, if the
        /// version string was valid, or <c>null</c> if the version string was not valid
        /// </param>
        ///
        /// <param name="strict">
        /// If <c>true</c>, minor and patch version are required, else they default to 0
        /// </param>
        ///
        /// <returns>
        /// Whether a valid version string was passed
        /// </returns>
        ///
        public static bool TryParse(string version, out SemVersion semver, bool strict = false)
        {
            try
            {
                semver = Parse(version, strict);
                return true;
            }
            catch (Exception)
            {
                semver = null;
                return false;
            }
        }


        /// <summary>
        /// Test two versions for equality
        /// </summary>
        ///
        /// <param name="versionA">
        /// A version
        /// </param>
        ///
        /// <param name="versionB">
        /// Another version
        /// </param>
        ///
        /// <returns>
        /// If versionA is equal to versionB <c>true</c>, else <c>false</c>
        /// </returns>
        ///
        public static bool Equals(SemVersion versionA, SemVersion versionB)
        {
            if (ReferenceEquals(versionA, null))
            {
                return ReferenceEquals(versionB, null);
            }

            return versionA.Equals(versionB);
        }


        /// <summary>
        /// Compare two versions
        /// </summary>
        ///
        /// <param name="versionA">
        /// A version
        /// </param>
        ///
        /// <param name="versionB">
        /// Another version to compare against
        /// </param>
        ///
        /// <returns>
        /// If versionA &lt; versionB <c>&lt; 0</c>, if versionA &gt; versionB <c>&gt; 0</c>,
        /// if versionA is equal to versionB <c>0</c>
        /// </returns>
        ///
        public static int Compare(SemVersion versionA, SemVersion versionB)
        {
            if (ReferenceEquals(versionA, null))
            {
                return ReferenceEquals(versionB, null) ? 0 : -1;
            }

            return versionA.CompareTo(versionB);
        }


        static int CompareComponent(string a, string b, bool lower = false)
        {
            var aEmpty = String.IsNullOrEmpty(a);
            var bEmpty = String.IsNullOrEmpty(b);
            if (aEmpty && bEmpty)
            {
                return 0;
            }

            if (aEmpty)
            {
                return lower ? 1 : -1;
            }

            if (bEmpty)
            {
                return lower ? -1 : 1;
            }

            var aComps = a.Split('.');
            var bComps = b.Split('.');

            var minLen = Math.Min(aComps.Length, bComps.Length);
            for (int i = 0; i < minLen; i++)
            {
                var ac = aComps[i];
                var bc = bComps[i];
                int anum, bnum;
                var isanum = Int32.TryParse(ac, out anum);
                var isbnum = Int32.TryParse(bc, out bnum);
                int r;
                if (isanum && isbnum)
                {
                    r = anum.CompareTo(bnum);
                    if (r != 0) return anum.CompareTo(bnum);
                }
                else
                {
                    if (isanum)
                        return -1;
                    if (isbnum)
                        return 1;
                    r = String.CompareOrdinal(ac, bc);
                    if (r != 0)
                        return r;
                }
            }

            return aComps.Length.CompareTo(bComps.Length);
        }


        /// <summary>
        /// Initialize a new semantic version from individual version component(s)
        /// </summary>
        ///
        /// <param name="major">
        /// The major version
        /// </param>
        ///
        /// <param name="minor">
        /// The minor version
        /// </param>
        ///
        /// <param name="patch">
        /// The patch version
        /// </param>
        ///
        /// <param name="prerelease">
        /// The prerelease version (eg. "alpha")
        /// </param>
        ///
        /// <param name="build">
        /// The build eg ("nightly.232")
        /// </param>
        ///
        public SemVersion(int major, int minor = 0, int patch = 0, string prerelease = "", string build = "")
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Prerelease = prerelease ?? "";
            Build = build ?? "";
        }


        /// <summary>
        /// Initialize a new semantic version from a .NET <see cref="Version"/>
        /// </summary>
        ///
        /// <param name="version">
        /// The .NET <see cref="Version"/> that is used to initialize the Major, Minor, Patch and Build properties
        /// </param>
        ///
        public SemVersion(Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            Major = version.Major;
            Minor = version.Minor;

            if (version.Revision >= 0)
            {
                Patch = version.Revision;
            }

            Prerelease = String.Empty;

            if (version.Build > 0)
            {
                Build = version.Build.ToString();
            }
            else
            {
                Build = String.Empty;
            }
        }


        /// <summary>
        /// Major version
        /// </summary>
        ///
        public int Major { get; private set; }


        /// <summary>
        /// Minor version
        /// </summary>
        ///
        public int Minor { get; private set; }


        /// <summary>
        /// Patch version
        /// </summary>
        ///
        public int Patch { get; private set; }


        /// <summary>
        /// Pre-release version
        /// </summary>
        ///
        public string Prerelease { get; private set; }


        /// <summary>
        /// Build version
        /// </summary>
        ///
        public string Build { get; private set; }


        /// <summary>
        /// Make a copy of the current instance with optional altered fields. 
        /// </summary>
        ///
        /// <param name="major">
        /// The major version
        /// </param>
        ///
        /// <param name="minor">
        /// The minor version
        /// </param>
        ///
        /// <param name="patch">
        /// The patch version
        /// </param>
        ///
        /// <param name="prerelease">
        /// The prerelease text
        /// </param>
        ///
        /// <param name="build">
        /// The build text
        /// </param>
        ///
        /// <returns>
        /// The new version object
        /// </returns>
        ///
        public SemVersion Change(
            int? major = null,
            int? minor = null,
            int? patch = null,
            string prerelease = null,
            string build = null
        )
        {
            return new SemVersion(
                major ?? Major,
                minor ?? Minor,
                patch ?? Patch,
                prerelease ?? Prerelease,
                build ?? Build);
        }


        /// <summary>
        /// Get a <see cref="string" /> that represents this instance
        /// </summary>
        ///
        public override string ToString()
        {
            var version = "" + Major + "." + Minor + "." + Patch;

            if (!string.IsNullOrEmpty(Prerelease))
            {
                version += "-" + Prerelease;
            }

            if (!string.IsNullOrEmpty(Build))
            {
                version += "+" + Build;
            }

            return version;
        }


        /// <summary>
        /// <see cref="IComparable.CompareTo(object)"/>
        /// </summary>
        ///
        public int CompareTo(object obj)
        {
            return CompareTo((SemVersion)obj);
        }


        /// <summary>
        /// <see cref="IComparable{T}.CompareTo(T)"/>
        /// </summary>
        ///
        public int CompareTo(SemVersion other)
        {
            if (ReferenceEquals(other, null))
            {
                return 1;
            }

            var r = CompareByPrecedence(other);
            if (r != 0)
            {
                return r;
            }

            r = CompareComponent(Build, other.Build);

            return r;
        }


        /// <summary>
        /// Compares to semantic versions by precedence
        /// </summary>
        ///
        /// <remarks>
        /// This does the same as a Equals, but ignores the build information.
        /// </remarks>
        ///
        /// <param name="other">
        /// The semantic version
        /// </param>
        ///
        /// <returns>
        /// <c>true</c> if the version precedence matches
        /// </returns>
        ///
        public bool PrecedenceMatches(SemVersion other)
        {
            return CompareByPrecedence(other) == 0;
        }


        /// <summary>
        /// Compares to semantic versions by precedence
        /// </summary>
        ///
        /// <remarks>
        /// This does the same as a Equals, but ignores the build information.
        /// </remarks>
        ///
        /// <param name="other">
        /// The semantic version
        /// </param>
        ///
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. 
        /// The return value has these meanings: Value Meaning Less than zero 
        /// This instance precedes <paramref name="other" /> in the version precedence.
        /// Zero This instance has the same precedence as <paramref name="other" />.
        /// Greater than zero This instance has creater precedence as <paramref name="other" />.
        /// </returns>
        ///
        public int CompareByPrecedence(SemVersion other)
        {
            if (ReferenceEquals(other, null))
            {
                return 1;
            }

            var r = Major.CompareTo(other.Major);
            if (r != 0)
            {
                return r;
            }

            r = Minor.CompareTo(other.Minor);
            if (r != 0)
            {
                return r;
            }

            r = Patch.CompareTo(other.Patch);
            if (r != 0)
            {
                return r;
            }

            r = CompareComponent(Prerelease, other.Prerelease, true);

            return r;
        }


        /// <summary>
        /// <see cref="object.Equals(object)"/>
        /// </summary>
        ///
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = (SemVersion)obj;

            return
                Major == other.Major &&
                Minor == other.Minor &&
                Patch == other.Patch &&
                string.Equals(Prerelease, other.Prerelease, StringComparison.Ordinal) &&
                string.Equals(Build, other.Build, StringComparison.Ordinal);
        }


        /// <summary>
        /// Produce a hash code for this instance
        /// </summary>
        ///
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table
        /// </returns>
        ///
        public override int GetHashCode()
        {
            unchecked
            {
                int result = Major.GetHashCode();
                result = result * 31 + Minor.GetHashCode();
                result = result * 31 + Patch.GetHashCode();
                result = result * 31 + Prerelease.GetHashCode();
                result = result * 31 + Build.GetHashCode();
                return result;
            }
        }


        /// <summary>
        /// Explicit conversion from string to SemVersion
        /// </summary>
        ///
        /// <param name="version">
        /// A string containing a semantic version
        /// </param>
        ///
        /// <returns>
        /// A <see cref="SemVersion"/> object matching <paramref name="version"/>
        /// - OR -
        /// <c>null</c> if <paramref name="version"/> was <c>null</c>
        /// </returns>
        ///
        public static explicit operator SemVersion(string version)
        {
            if (version == null)
            {
                return null;
            }

            return Parse(version);
        }

        /// <summary>
        /// Implicit conversion from SemVersion to string
        /// </summary>
        ///
        /// <param name="version">
        /// A SemVersion
        /// </param>
        ///
        /// <returns>
        /// String representation of <paramref name="version"/>
        /// - OR -
        /// <c>null</c> if <paramref name="version"/> was <c>null</c>
        /// </returns>
        ///
        public static implicit operator string(SemVersion version)
        {
            if (version == null)
            {
                return null;
            }

            return version.ToString();
        }


        public static bool operator ==(SemVersion left, SemVersion right)
        {
            return Equals(left, right);
        }


        public static bool operator !=(SemVersion left, SemVersion right)
        {
            return !Equals(left, right);
        }


        public static bool operator >(SemVersion left, SemVersion right)
        {
            return Compare(left, right) > 0;
        }


        public static bool operator >=(SemVersion left, SemVersion right)
        {
            return left == right || left > right;
        }


        public static bool operator <(SemVersion left, SemVersion right)
        {
            return Compare(left, right) < 0;
        }


        public static bool operator <=(SemVersion left, SemVersion right)
        {
            return left == right || left < right;
        }

    }
}
