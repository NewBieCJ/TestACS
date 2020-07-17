﻿namespace Win32
{
	/// <summary>
	/// Specifies return values for <see cref="O:Win32.DisplayMode.ChangeDisplaySettings">DisplayMode.ChangeDisplaySettings</see>.
	/// </summary>
	public enum DISP_CHANGE : int
	{
		/// <summary>
		/// The settings change was successful.
		/// </summary>
		SUCCESSFUL=0,

		/// <summary>
		/// The computer must be restarted for the graphics mode to work.
		/// </summary>
		RESTART=1,

		/// <summary>
		/// The display driver failed the specified graphics mode.
		/// </summary>
		FAILED=-1,

		/// <summary>
		/// The graphics mode is not supported.
		/// </summary>
		BADMODE=-2,

		/// <summary>
		/// Unable to write settings to the registry.
		/// </summary>
		NOTUPDATED=-3,

		/// <summary>
		/// An invalid set of flags was passed in.
		/// </summary>
		BADFLAGS=-4,

		/// <summary>
		/// An invalid parameter was passed in. This can include an invalid flag or combination of flags.
		/// </summary>
		BADPARAM=-5,

		/// <summary>
		/// The settings change was unsuccessful because the system is DualView capable.
		/// </summary>
		BADDUALVIEW=-6,
	}
}
