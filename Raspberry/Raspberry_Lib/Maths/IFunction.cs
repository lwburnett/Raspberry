namespace Raspberry_Lib.Maths
{
    internal interface IFunction
    {
        /// <summary>
        /// For this function, gives you the y value for a given x AKA F(x)
        /// </summary>
        /// <param name="iX">The value for x for which you want y</param>
        /// <returns>The y value for a given x AKA F(x)</returns>
        float GetYForX(float iX);

        /// <summary>
        /// For this function, give you the slope for a given x AKA F'(x)
        /// </summary>
        /// <param name="iX">The value for x for which you want the slope</param>
        /// <returns>The slope for a given x AKA F'(x)</returns>
        float GetYPrimeForX(float iX);

        /// <summary>
        /// Beginning X value of this function's domain
        /// </summary>
        float DomainStart { get; }

        /// <summary>
        /// Ending X value of this function's domain
        /// </summary>
        float DomainEnd { get; }
    }
}
