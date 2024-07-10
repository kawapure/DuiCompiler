// This file will be overridden by a script that runs at build time.
// For more information, please view the .csproj file.

namespace DuiCompiler
{
	public static class BuildNumber
	{
	    /// <summary>
		/// The number of ticks that have passed since the file was built.
		/// </summary>
		public const long COMPILE_TIME = 0;
		
		/// <summary>
		/// The build number.
		/// </summary>
		/// <remarks>
		/// This, like all Network Neighborhood software, is equivalent to the
		/// number of days that have passed since the application started
		/// development.
		/// </remarks>
		public const int BUILD_NUMBER = 0;
	}
}
