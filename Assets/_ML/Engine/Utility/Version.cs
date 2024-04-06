using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ML.Engine.Utility
{
    [System.Serializable, LabelText("°æ±¾ºÅ")]
    public struct Version
    {
        [LabelText("Ö÷°æ±¾ºÅ")]
        public int Major;
        [LabelText("´Î°æ±¾ºÅ")]
        public int Minor;
        [LabelText("ÐÞ¶©°æ±¾ºÅ")]
        public int Patch;

        public Version(string version)
        {
            string[] versions = version.Split('.');
            Major = int.Parse(versions[0]);
            Minor = int.Parse(versions[1]);
            Patch = int.Parse(versions[2]);
        }
        public Version(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public override bool Equals(object obj)
        {
            if (obj is Version other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Major, Minor, Patch);
        }

        public static bool operator ==(Version A, Version B)
        {
            return A.Major == B.Major && A.Minor == B.Minor && A.Patch == B.Patch;
        }
        public static bool operator !=(Version A, Version B)
        {
            return !(A == B);
        }

        public static bool operator >(Version A, Version B)
        {
            if (A.Major != B.Major)
            {
                return A.Major > B.Major;
            }
            else if (A.Minor != B.Minor)
            {
                return A.Minor > B.Minor;
            }
            else
            {
                return A.Patch > B.Patch;
            }
        }
        public static bool operator >=(Version A, Version B)
        {
            return A.Major >= B.Major || A.Minor >= B.Minor || A.Patch >= B.Patch;
        }

        public static bool operator <(Version A, Version B)
        {
            if (A.Major != B.Major)
            {
                return A.Major < B.Major;
            }
            else if (A.Minor != B.Minor)
            {
                return A.Minor < B.Minor;
            }
            else
            {
                return A.Patch < B.Patch;
            }
        }
        public static bool operator <=(Version A, Version B)
        {
            return A.Major <= B.Major || A.Minor <= B.Minor || A.Patch <= B.Patch;
        }
        

        public override string ToString()
        {
            return $"{Major}.{Minor}.{Patch}";
        }
    }
}

