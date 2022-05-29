namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;

public interface ISequenceNumberGenerator {
    int NewSequenceNumber(string sequenceName);
}