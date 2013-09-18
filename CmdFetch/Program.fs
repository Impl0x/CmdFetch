open System
open System.Drawing
open System.Drawing.Imaging
open System.IO
open System.Text.RegularExpressions
open System.Threading
open System.Windows.Forms

open Microsoft.VisualBasic

/// Instance of the Visual Basic Computer class, used for retrieving core system traits.
let computer = new Devices.Computer ()

let mutable regKey = computer.Registry.LocalMachine.OpenSubKey "HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0"

/// String containing the name of the processor.
let cpuType = Regex.Replace (string (regKey.GetValue "ProcessorNameString"), "\s+", " ")
let cpuIdent = string (regKey.GetValue "Identifier")

regKey <- computer.Registry.LocalMachine.OpenSubKey "HARDWARE\\DESCRIPTION\\System\\BIOS"

/// Information consisting of the vendor, version, and release date of the BIOS.
let bios = (string (regKey.GetValue "BIOSVendor")) + " " + (string (regKey.GetValue "BIOSVersion")) + " (" + (string (regKey.GetValue "BIOSReleaseDate")) + ")"

let mobo = (string (regKey.GetValue "SystemManufacturer")) + " - " + (string (regKey.GetValue "SystemProductName"))

/// Instantiation of the DriveInfo class used to represent the C: drive.
let primaryDrive = (DriveInfo.GetDrives () |> Array.filter (fun (drive : DriveInfo) -> drive.Name = "C:\\" && drive.IsReady)).[0]

/// Total storage space (in gigabytes) of the C:\ drive
let totalSpace = (primaryDrive.TotalSize / int64 1073741824)

/// Amount of storage space of the C:\ drive (in gigabytes) that is already in use.
let usedSpace = (totalSpace - (primaryDrive.AvailableFreeSpace / int64 1073741824))

/// System.Drawing.Rectangle used to store the dimensions of the primary display screen.
let screen = Screen.PrimaryScreen.Bounds

/// Total RAM (in megabytes) that is available to the system.
let totalRAM = computer.Info.TotalPhysicalMemory / 1048576UL

/// RAM (in megabytes) that is currently being allocated by the system.
let usedRAM =  totalRAM - (computer.Info.AvailablePhysicalMemory / 1048576UL)

/// Shows whether the OS is 32-bit or 64-bit.
let OSBits =
    match Environment.Is64BitOperatingSystem with
    | true -> "x64"
    | false -> "x32"

/// Full name of the operating system, including the architecture (32- vs. 64-bit)
let OSName = computer.Info.OSFullName + OSBits

/// Kernel version of the computer.Info.
let kernel = computer.Info.OSVersion

let machineName = Environment.UserName + "@" + Environment.MachineName

/// Gets the time since the computer was last retarded.
let uptime () = 
    let millis = Environment.TickCount
    let seconds = (millis / 1000) % 60 |> string
    let minutes = (millis / (1000 * 60)) % 60 |> string
    let hours = (millis / (1000 * 60 * 60)) % 24 |> string
    hours + "hrs "  + minutes + "mins " + seconds + "secs"

/// Captures a picture of the whole screen and saves the image to the desktop.
let saveScreenShot () =
    let screenBitmap = new Bitmap (screen.Width, screen.Height, PixelFormat.Format32bppArgb)
    let screenGraphicsObject = Graphics.FromImage (screenBitmap)
    screenGraphicsObject.CopyFromScreen (screen.X, screen.Y, 0, 0, screen.Size, CopyPixelOperation.SourceCopy)
    screenBitmap.Save ((Environment.GetFolderPath Environment.SpecialFolder.Desktop) + @"\screenShot.png", ImageFormat.Png)

let rawFlag = "\n
         #R,.=:!!t3Z3z.,               #w " + machineName + "}
        #R:tt:::tt333EE3                #gOS:#w " + OSName + "}
        #REt:::ztt33EEE  #G@Ee.,      .., #gKernel version:#w " + kernel + "}
       #R;tt:::tt333EE7 #G;EEEEEEttttt33@ #gUptime:#w " + (uptime ()) + "}
      #R:Et:::zt333EEQ. #GSEEEEEttttt33QL #gResolution:#w " + string screen.Width + "x" + string screen.Height + "}
      #Rit::::tt333EEF #G@EEEEEEttttt33F  #gRAM(Used):#w " + string usedRAM + "/" + string totalRAM + " MB}
     #R;3=*^```'*4EEV #G:EEEEEEttttt33@. #gBIOS:#w " + bios + "}
     #B,.=::::it=., #R` #G@EEEEEEtttz33QF #gMoBo:#w " + mobo + "}
    #B;::::::::zt33)   #G'4EEEtttji3P* #gCPU:#w " + cpuType + "}
   #B:t::::::::tt33.#Y:Z3z..  #G``       #gCPU ID:#w " + cpuIdent + "}
   #Bi::::::::zt33F #YAEEEtttt::::ztF #gHard Drive(Used):#w " + string usedSpace + "/" + string totalSpace + " GB}
  #B;:::::::::t33V #Y;EEEttttt::::t3      #g#w }
  #BE::::::::zt33L #Y@EEEtttt::::z3F      #g#w }
 #B{3=*^```'*4E3) #Y;EEEtttt:::::tZ`      #g#w }
             #B` #Y:EEEEtttt::::z7 #g#w " + DateTime.UtcNow.ToString ("s", System.Globalization.CultureInfo.InvariantCulture) + "}
                 #Y'VEzjt:;;z>*`\n\n"

let run () =
    let flagLines = rawFlag.Split '}'

    for line in flagLines do
        Thread.Sleep 50
        let subLines = line.Split '#'
        for subLine in subLines do
            if subLine <> "" then
                match subLine.[0] with
                | 'w' ->
                    Console.ForegroundColor <- ConsoleColor.White
                    Console.Write (Regex.Replace (subLine, "w ", " "))
                | 'g' -> 
                    Console.ForegroundColor <- ConsoleColor.Gray
                    Console.Write (Regex.Replace (subLine, "g", ""))
                | 'R' ->
                    Console.ForegroundColor <- ConsoleColor.Red
                    Console.Write (Regex.Replace (subLine, "R", ""))
                | 'B' ->
                    Console.ForegroundColor <- ConsoleColor.Blue
                    Console.Write (Regex.Replace (subLine, "B", ""))
                | 'G' ->
                    Console.ForegroundColor <- ConsoleColor.Green
                    Console.Write (Regex.Replace (subLine, "G", ""))
                | 'Y' ->
                    Console.ForegroundColor <- ConsoleColor.Yellow
                    Console.Write (Regex.Replace (subLine, "Y", ""))
                | _ -> 
                    Console.Write subLine

[<EntryPoint>]
let main args =
    //Expand console width to accomodate some lines
    Console.BufferWidth <- Console.BufferWidth + 1
    Console.WindowWidth <- Console.WindowWidth + 1

    //Store current text color so that it can be reverted at end of program.
    let color = Console.ForegroundColor
    run ()
    Console.ForegroundColor <- color

    if args = [|"-s"|] then
        printf "Taking screenshot in "
        for i = 5 downto 1 do
            printf "%i... " i
            Thread.Sleep 1000
        saveScreenShot ()

    Console.WindowWidth <- Console.WindowWidth - 1
    Console.BufferWidth <- Console.BufferWidth - 1
    0
