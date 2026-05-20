using System;
using System.Linq;
using Projet_api2.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Projet_api2.Services;

public class PdfExportService
{
    private readonly ProjetService _projetService;
    private readonly TacheService _tacheService;

    public PdfExportService(ProjetService projetService, TacheService tacheService)
    {
        _projetService = projetService;
        _tacheService = tacheService;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateProjectReport(int projectId)
    {
        var projet = _projetService.GetProjetById(projectId);
        if (projet == null) return Array.Empty<byte>();

        var membres = _projetService.GetMembresByProjectId(projectId);
        var taches = _tacheService.GetTachesByProjectId(projectId);

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30, Unit.Millimetre);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("PMANAGER").FontSize(8).FontColor("#7F8C8D").Bold();
                            c.Item().Text("Fiche de suivi de projet").FontSize(18).FontColor("#1E3A5F").Bold();
                            c.Item().Text(projet.Nom.ToUpper()).FontSize(13).FontColor("#2E86AB").Bold();
                        });
                        row.ConstantItem(100).AlignRight().Column(c =>
                        {
                            c.Item().Text("Généré le").FontSize(8).FontColor("#95A5A6");
                            c.Item().Text(DateTime.Now.ToString("dd/MM/yyyy")).FontSize(8).FontColor("#7F8C8D");
                        });
                    });

                    col.Item().PaddingTop(4).LineHorizontal(2).LineColor("#2E86AB");
                    col.Item().PaddingBottom(8);
                });

                page.Content().Column(col =>
                {
                    col.Item().Background("#D6EAF8").Padding(10).Column(info =>
                    {
                        info.Item().Text("Informations du projet").Bold().FontColor("#1E3A5F");
                        info.Item().PaddingTop(4).Row(row =>
                        {
                            row.ConstantItem(120).Text("Chef de Projet :").Bold();
                            row.RelativeItem().Text(projet.NomCreateur ?? "-");
                        });
                        info.Item().Row(row =>
                        {
                            row.ConstantItem(120).Text("Statut :").Bold();
                            row.RelativeItem().Text(projet.Status.ToString());
                        });
                        info.Item().Row(row =>
                        {
                            row.ConstantItem(120).Text("Date de création :").Bold();
                            row.RelativeItem().Text(projet.DateCreation.ToString("dd/MM/yyyy"));
                        });
                        info.Item().Row(row =>
                        {
                            row.ConstantItem(120).Text("Description :").Bold();
                            row.RelativeItem().Text(projet.Description);
                        });
                    });

                    col.Item().PaddingVertical(12).Text("Composition de l'équipe").FontSize(14).Bold().FontColor("#1E3A5F");

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(2); 
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(3); 
                        });

                        static IContainer HeaderCell(IContainer c) => c.Background("#1E3A5F").Padding(6).AlignMiddle();

                        table.Header(h =>
                        {
                            h.Cell().Element(HeaderCell).Text("Prénom & Nom").Bold().FontColor("#FFFFFF");
                            h.Cell().Element(HeaderCell).Text("Email").Bold().FontColor("#FFFFFF");
                            h.Cell().Element(HeaderCell).Text("Rôle dans le projet").Bold().FontColor("#FFFFFF");
                        });

                        if (!membres.Any())
                        {
                            table.Cell().ColumnSpan(3).Padding(6).Text("Aucun collaborateur assigné.").FontColor("#7F8C8D");
                        }
                        else
                        {
                            var rowIndex = 0;
                            foreach (var m in membres)
                            {
                                var bg = rowIndex % 2 == 0 ? "#F2F4F7" : "#FFFFFF";
                                rowIndex++;

                                IContainer DataCell(IContainer c) => c.Background(bg).Padding(6).AlignMiddle();

                                table.Cell().Element(DataCell).Text($"{m.Membre.Prenom} {m.Membre.Nom}");
                                table.Cell().Element(DataCell).Text(m.Membre.Email);
                                table.Cell().Element(DataCell).Text(m.Role);
                            }
                        }
                    });

                    col.Item().PaddingVertical(12).Text("Suivi des tâches").FontSize(14).Bold().FontColor("#1E3A5F");

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(4);
                            cols.RelativeColumn(2);
                            cols.ConstantColumn(80);
                        });

                        static IContainer HeaderCell(IContainer c) => c.Background("#1E3A5F").Padding(6).AlignMiddle();

                        table.Header(h =>
                        {
                            h.Cell().Element(HeaderCell).Text("Titre de la tâche").Bold().FontColor("#FFFFFF");
                            h.Cell().Element(HeaderCell).Text("Assignée à").Bold().FontColor("#FFFFFF");
                            h.Cell().Element(HeaderCell).Text("Statut").Bold().FontColor("#FFFFFF");
                        });

                        if (!taches.Any())
                        {
                            table.Cell().ColumnSpan(3).Padding(6).Text("Aucune tâche enregistrée.").FontColor("#7F8C8D");
                        }
                        else
                        {
                            var rowIndex = 0;
                            foreach (var t in taches)
                            {
                                var bg = rowIndex % 2 == 0 ? "#F2F4F7" : "#FFFFFF";
                                rowIndex++;

                                IContainer DataCell(IContainer c) => c.Background(bg).Padding(6).AlignMiddle();

                                table.Cell().Element(DataCell).Text(t.Titre);
                                table.Cell().Element(DataCell).Text(t.NomAssigne);
                                
                                string statusText = t.IsCompleted ? "Terminée" : "En cours";
                                string statusColor = t.IsCompleted ? "#1A7A4A" : "#2E86AB";
                                
                                table.Cell().Element(DataCell).Text(statusText).FontColor(statusColor).Bold();
                            }
                        }
                    });

                    var totalTasks = taches.Count;
                    var totalDone = taches.Count(t => t.IsCompleted);
                    var pct = totalTasks > 0 ? (int)(totalDone * 100.0 / totalTasks) : 0;

                    col.Item().PaddingTop(16).Background("#F2F4F7").Padding(10).Column(sum =>
                    {
                        sum.Item().Text("Avancement global").Bold().FontColor("#1E3A5F");
                        sum.Item().PaddingTop(4).Row(row =>
                        {
                            row.ConstantItem(160).Text($"Tâches complétées : {totalDone} / {totalTasks}");
                            row.RelativeItem().Text($"{pct}%").Bold().FontColor(pct == 100 ? "#1A7A4A" : "#2E86AB");
                        });
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("PManager  -  Page ").FontColor("#95A5A6").FontSize(8);
                    text.CurrentPageNumber().FontSize(8);
                    text.Span(" / ").FontSize(8);
                    text.TotalPages().FontSize(8);
                });
            });
        });

        return doc.GeneratePdf();
    }
}