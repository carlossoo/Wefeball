namespace Game.Wind
{
    /// <summary>
    /// Os 3 níveis de força do vento. O jogador só alterna entre estes 3 (tecla Shift, em ciclo).
    /// Os VALORES de cada nível (velocidade em m/s) ficam no asset WindSettings, não aqui.
    /// </summary>
    public enum WindStrength
    {
        Weak = 0,   // Fraca
        Medium = 1, // Média
        Strong = 2  // Forte
    }
}
