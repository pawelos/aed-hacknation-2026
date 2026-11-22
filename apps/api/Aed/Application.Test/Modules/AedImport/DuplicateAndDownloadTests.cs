using System.Net;
using Application.Modules.AedImport;
using Infrastructure.Import;

namespace Application.Test.Modules.AedImport;

/// <summary>
/// Possible duplicates between sources and the address check of the URL import.
/// </summary>
public sealed class DuplicateAndDownloadTests
{
    [TestCase("Ratusz Wrocław – AED w holu", "Sukiennice 9", "Urząd Miejski – Nowy Ratusz", "x", false)]
    [TestCase("BOM Zapolskiej", "Zapolskiej 4", "Biuro Obsługi Mieszkańców", "ul. Zapolskiej 4", true)]
    [TestCase("Dworzec PKP", "Dworcowa 1", "Dworzec kolejowy PKP", "ul. Inna 2", true)]
    [TestCase("Szkoła nr 5", "Szkolna 5", "Basen miejski", "Wodna 2", false)]
    [TestCase("Sklep", "Sukiennice 9", "Apteka", "Sukiennice 1", false)]
    public void Similar_name_or_same_address(string nameA, string addressA, string nameB, string addressB, bool expected)
    {
        Assert.That(DuplicateMatcher.IsSimilar(nameA, addressA, nameB, addressB), Is.EqualTo(expected));
    }

    [TestCase("127.0.0.1", false)]
    [TestCase("10.1.2.3", false)]
    [TestCase("172.20.0.5", false)]
    [TestCase("192.168.1.16", false)]
    [TestCase("169.254.169.254", false)]
    [TestCase("100.64.0.1", false)]
    [TestCase("::1", false)]
    [TestCase("fd00::1", false)]
    [TestCase("::ffff:10.0.0.1", false)]
    [TestCase("152.228.135.251", true)]
    [TestCase("2a00:1450:4001::200e", true)]
    public void Only_public_addresses_can_be_downloaded(string address, bool expected)
    {
        Assert.That(ImportDownloader.IsPublic(IPAddress.Parse(address)), Is.EqualTo(expected));
    }
}
