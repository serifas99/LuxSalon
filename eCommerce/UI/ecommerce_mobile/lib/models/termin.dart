import 'package:json_annotation/json_annotation.dart';

part 'termin.g.dart';

@JsonSerializable()
class Termin {
  final int? id;
  final int? klijentId;
  final String? klijentImePrezime;
  final int? frizerId;
  final String? frizerImePrezime;
  final int? uslugaId;
  final String? uslugaNaziv;
  final DateTime? datumVrijeme;
  final int? trajanjeMinuta;
  final double? cijena;
  final String? status;

  /// Status vezanog Placanja (npr. "Zavrseno") ako postoji, inace null - koristi se
  /// da se odluci da li jos treba prikazati dugme "Plati" (termin moze biti
  /// "Potvrdjen" i bez placanja, ako ga frizer/admin rucno potvrdi).
  final String? placanjeStatus;
  final String? napomena;
  final DateTime? createdAt;
  final DateTime? updatedAt;

  Termin({
    this.id,
    this.klijentId,
    this.klijentImePrezime,
    this.frizerId,
    this.frizerImePrezime,
    this.uslugaId,
    this.uslugaNaziv,
    this.datumVrijeme,
    this.trajanjeMinuta,
    this.cijena,
    this.status,
    this.placanjeStatus,
    this.napomena,
    this.createdAt,
    this.updatedAt,
  });

  factory Termin.fromJson(Map<String, dynamic> json) =>
      _$TerminFromJson(json);

  Map<String, dynamic> toJson() => _$TerminToJson(this);
}
