namespace Game.Core
{
    /// <summary>
    /// As 3 formas que o player pode assumir.
    /// A ORDEM importa: é a ordem do ciclo (Bola > Pena > Peso > Bola).
    /// Se for adicionar uma forma nova, coloque no final do enum.
    /// </summary>
    public enum PlayerForm
    {
        Ball = 0,    // Bola: forma mediana, pode pular
        Feather = 1, // Pena: leve, flutua com o vento
        Weight = 2   // Peso: pesado, praticamente ignora o vento
    }
}
