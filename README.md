# WPF Email Client

A modern, feature-rich email client built with WPF (.NET 6+), leveraging MailKit for email protocols, MVVM architecture with CommunityToolkit, and Gong-WPF-DragDrop for enhanced user interactions.

## Features

- Support for IMAP and SMTP protocols
- Rich email previews
- Comprehensive folder management
- Drag-and-drop functionality
- Modern UI with WPF
- MVVM architecture for clean, maintainable code

## Technologies Used

- WPF (.NET 6+)
- MailKit
- MVVM (CommunityToolkit.Mvvm)
- Gong-WPF-DragDrop

## Prerequisites

- .NET 6.0 SDK or later
- Visual Studio 2022 (recommended)

## Installation

1. Clone the repository
2. Open the solution in Visual Studio 2022
3. Restore NuGet packages
4. Build and run the project

## Configuration

Update the appsettings.json to have the access to the database for storing session data (and reconfigure Application Database Context according to your DB).

## Usage

1. Launch the application
2. Enter your email credentials
3. The main interface will display your email folders and messages
4. Use the rich preview pane to view email content
5. Manage folders using the folder tree view
6. Drag and drop emails between folders for easy organization

## Project Structure

- `Models/`: Contains data models for emails, folders, and accounts
- `ViewModels/`: MVVM ViewModels for each view
- `Views/`: WPF Views (XAML) for the user interface
- `Services/`: Email services for IMAP and SMTP operations
- `Helpers/`: Utility classes and helper functions

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Acknowledgments

- [MailKit](https://github.com/jstedfast/MailKit) for robust email protocol support
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) for MVVM implementation
- [Gong-WPF-DragDrop](https://github.com/punker76/gong-wpf-dragdrop) for drag-and-drop functionality

## Contact

If you have any questions or suggestions, please open an issue in the GitHub repository or contact me at the email in my profile.
