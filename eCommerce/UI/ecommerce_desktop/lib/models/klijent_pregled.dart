class KlijentPregled {
  final int? id;
  final String? imePrezime;
  final String? email;
  final int? brojZakazanihTermina;
  final DateTime? datumPosljednjegTermina;

  KlijentPregled({
    this.id,
    this.imePrezime,
    this.email,
    this.brojZakazanihTermina,
    this.datumPosljednjegTermina,
  });

  factory KlijentPregled.fromJson(Map<String, dynamic> json) => KlijentPregled(
        id: (json['id'] as num?)?.toInt(),
        imePrezime: json['imePrezime'] as String?,
        email: json['email'] as String?,
        brojZakazanihTermina: (json['brojZakazanihTermina'] as num?)?.toInt(),
        datumPosljednjegTermina: json['datumPosljednjegTermina'] == null
            ? null
            : DateTime.parse(json['datumPosljednjegTermina'] as String),
      );
}
