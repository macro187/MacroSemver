$src = "..\..\test\Semver.Test\SemVersionTest.cs"
$src = [IO.Path]::GetFullPath($src)

$dest = [IO.Path]::Combine([Environment]::CurrentDirectory, "SemVersionTest.mstest.cs")
[IO.File]::Delete($dest)
[IO.File]::Copy($src, $dest)

$s = [IO.File]::ReadAllText($dest)
$s = $s.Replace("using Xunit;", "using Microsoft.VisualStudio.TestTools.UnitTesting;")
$s = $s.Replace("public class", "[TestClass] public class")
$s = $s.Replace("[Fact]", "[TestMethod]")
$s = $s.Replace(".True(", ".IsTrue(")
$s = $s.Replace(".False(", ".IsFalse(")
$s = $s.Replace(".Equal(", ".AreEqual(")
$s = $s.Replace(".NotEqual(", ".AreNotEqual(")
$s = $s.Replace("Assert.Throws(", "AssertExtensions.Throws(")
$s = $s.Replace("Assert.Throws<", "AssertExtensions.Throws<")
[IO.File]::WriteAllText($dest, $s)

