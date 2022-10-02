using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;

namespace Place;

public static class Program
{
    private static readonly Dictionary<uint, string> MappedPages = new Dictionary<uint, string>();
    
    private static readonly string RawPagesDir = Path.Combine("raw", "pages");
    
    private static readonly string RawAssetsDir = Path.Combine("raw", "static", "assets");

    private static readonly string StaticPagesDir = Path.Combine("static", "pages");
    
    private static readonly string StaticAssetsDir = Path.Combine("static", "assets");
    
    public static void Main(string[] args)
    {
        SetUp();
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

    public static bool TryGetMappedPage(uint id, out string page)
    {
        return MappedPages.TryGetValue(id, out page!);
    }
    
    public static bool TryGetPage(uint year, uint month, uint day, string name, out string path)
    {
        path = Path.GetFullPath(Path.Combine(StaticPagesDir,
            $"{year:D4}{Path.DirectorySeparatorChar}{month:D2}{Path.DirectorySeparatorChar}{day:D2}{Path.DirectorySeparatorChar}{name}.html"));

        return File.Exists(path);
    }

    public static bool TryGetAsset(uint id, string ext, out string path)
    {
        path = Path.GetFullPath(Path.Combine(StaticAssetsDir, $"{id}.{ext}"));

        return File.Exists(path);
    }

    private static void SetUp()
    {
        uint pageCount = 0;

        // generate pages
        if (Directory.Exists(RawPagesDir))
        {
            foreach (var year in Directory.EnumerateDirectories(RawPagesDir))
            {
                foreach (var month in Directory.EnumerateDirectories(year))
                {
                    foreach (var day in Directory.EnumerateDirectories(month))
                    {
                        foreach (var name in Directory.EnumerateFiles(day))
                        {
                            string page = name[RawPagesDir.Length..^Path.GetExtension(name).Length];

                            string staticPage = StaticPagesDir + Path.GetDirectoryName(page);
                            Directory.CreateDirectory(staticPage);
                            
                            var document = MarkdownParser.Parse(File.ReadAllText(name));

                            File.WriteAllText(StaticPagesDir + page + ".html", document.ToHtml());
                            
                            MappedPages.Add(pageCount, page);
                            pageCount++;
                        }
                    }
                }
            }
        }

        if (Directory.Exists(RawAssetsDir))
        {
            Directory.CreateDirectory(StaticAssetsDir);
            
            foreach (var asset in Directory.EnumerateFiles(RawAssetsDir))
            {
                File.Copy(asset, StaticAssetsDir + asset[RawAssetsDir.Length..], true);
            }
        }
    }
}